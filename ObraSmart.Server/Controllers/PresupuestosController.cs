using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObraSmart.Application.DTOs.Presupuestos;
using ObraSmart.Application.Interfaces.Services;

namespace ObraSmart.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PresupuestosController : ControllerBase
    {
        private readonly IPresupuestoService _presupuestoService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<PresupuestoUpsertDto> _validator;

        public PresupuestosController(
            IPresupuestoService presupuestoService,
            ICurrentUserService currentUserService,
            IValidator<PresupuestoUpsertDto> validator)
        {
            _presupuestoService = presupuestoService;
            _currentUserService = currentUserService;
            _validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PresupuestoDto>>> ObtenerTodos()
        {
            var result = await _presupuestoService.ObtenerTodosAsync();

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PresupuestoDto>> ObtenerPorId(Guid id)
        {
            var result = await _presupuestoService.ObtenerPorIdAsync(id);

            if (!result.IsSuccess)
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Crear([FromBody] PresupuestoUpsertDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }

            var usuarioId = _currentUserService.GetUserId();
            if (usuarioId == null || usuarioId == Guid.Empty)
                return Unauthorized(new { message = "Usuario no autenticado." });

            var result = await _presupuestoService.CrearAsync(dto, usuarioId.Value);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });

            return CreatedAtAction(nameof(ObtenerPorId), new { id = result.Data }, result.Data);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Actualizar(Guid id, [FromBody] PresupuestoUpsertDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }

            var usuarioId = _currentUserService.GetUserId();
            if (usuarioId == null || usuarioId == Guid.Empty)
                return Unauthorized(new { message = "Usuario no autenticado." });

            var result = await _presupuestoService.ActualizarAsync(id, dto, usuarioId.Value);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            var result = await _presupuestoService.EliminarAsync(id);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });

            return NoContent();
        }
    }
}
