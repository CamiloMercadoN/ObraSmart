using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObraSmart.Application.DTOs.Clientes;
using ObraSmart.Application.Interfaces;
using System.Security.Claims;

namespace ObraSmart.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        private Guid ObtenerUsuarioId()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("Sub");
            if (claimId == null || !Guid.TryParse(claimId.Value, out Guid usuarioId))
            {
                throw new UnauthorizedAccessException("ID de usuario no válido en el token.");
            }
            return usuarioId;
        }

        [HttpGet]
        public async Task<IActionResult> GetClientes()
        {
            var usuarioId = ObtenerUsuarioId();
            var result = await _clienteService.ObtenerTodosAsync(usuarioId);

            if (!result.IsSuccess)
                return BadRequest(new { Error = result.ErrorMessage });

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCliente(Guid id)
        {
            var usuarioId = ObtenerUsuarioId();
            var result = await _clienteService.ObtenerPorIdAsync(id, usuarioId);

            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "NOT_FOUND") return NotFound(new { Error = result.ErrorMessage });
                return BadRequest(new { Error = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> PostCliente([FromBody] ClienteRequestDto dto)
        {
            var usuarioId = ObtenerUsuarioId();
            var result = await _clienteService.CrearAsync(dto, usuarioId);

            if (!result.IsSuccess)
                return BadRequest(new { Error = result.ErrorMessage });

            return CreatedAtAction(nameof(GetCliente), new { id = result.Data.Id }, result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(Guid id, [FromBody] ClienteRequestDto dto)
        {
            var usuarioId = ObtenerUsuarioId();
            var result = await _clienteService.ActualizarAsync(id, dto, usuarioId);

            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "NOT_FOUND") return NotFound(new { Error = result.ErrorMessage });
                return BadRequest(new { Error = result.ErrorMessage });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(Guid id)
        {
            var usuarioId = ObtenerUsuarioId();
            var result = await _clienteService.EliminarAsync(id, usuarioId);

            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "NOT_FOUND") return NotFound(new { Error = result.ErrorMessage });
                if (result.ErrorCode == "CONFLICT") return Conflict(new { Error = result.ErrorMessage });
                return BadRequest(new { Error = result.ErrorMessage });
            }

            return NoContent();
        }
    }
}
