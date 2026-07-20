using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObraSmart.Application.DTOs.APUs;
using ObraSmart.Application.Interfaces.Services;

namespace ObraSmart.Server.Controllers
{
    [Route("api/apus")]
    [ApiController]
    [Authorize]
    public class EstructuraAPUController : ControllerBase
    {
        private readonly IEstructuraAPUService _apuService;
        private readonly IValidator<EstructuraAPUUpsertDto> _validator;

        public EstructuraAPUController(
            IEstructuraAPUService apuService,
            IValidator<EstructuraAPUUpsertDto> validator)
        {
            _apuService = apuService;
            _validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstructuraAPUDto>>> ObtenerTodos()
        {
            var result = await _apuService.ObtenerTodosAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return Ok(result.Data);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EstructuraAPUDto>> ObtenerPorId(Guid id)
        {
            var result = await _apuService.ObtenerPorIdAsync(id);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Crear([FromBody] EstructuraAPUUpsertDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Error = validationResult.Errors.First().ErrorMessage });
            }

            var result = await _apuService.CrearAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            // Devuelve 201 Created
            return CreatedAtAction(nameof(ObtenerPorId), new { id = result.Data }, result.Data);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Actualizar(Guid id, [FromBody] EstructuraAPUUpsertDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Error = validationResult.Errors.First().ErrorMessage });
            }

            var result = await _apuService.ActualizarAsync(id, dto);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Eliminar(Guid id)
        {
            var result = await _apuService.EliminarAsync(id);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return NoContent();
        }

        // Endpoint específico para forzar el recálculo con los precios de mercado actuales
        [HttpPatch("{id:guid}/recalcular")]
        public async Task<ActionResult> RecalcularCosto(Guid id)
        {
            var result = await _apuService.RecalcularCostoExplicitoAsync(id);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return NoContent();
        }
    }
}
