using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObraSmart.Application.DTOs.ConfiguracionComercial;
using ObraSmart.Application.Interfaces.Services;
using System.Security.Claims;

namespace ObraSmart.Server.Controllers
{
    [ApiController]
    [Route("api/configuracion-comercial")]
    [Authorize]
    public class ConfiguracionComercialController(
            IConfiguracionComercialService configuracionService,
            IValidator<ConfiguracionComercialDto> validator) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ConfiguracionComercialDto>> Obtener()
        {
            // Extraer el ID del usuario desde el Token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid usuarioId))
            {
                return Unauthorized(new { ErrorMessage = "Usuario no autorizado.", ErrorCode = "UNAUTHORIZED" });
            }

            var result = await configuracionService.ObtenerPorUsuarioAsync(usuarioId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { result.ErrorMessage, result.ErrorCode });
            }

            return result.Data;
        }

        [HttpPut]
        public async Task<IActionResult> Guardar([FromBody] ConfiguracionComercialDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid usuarioId))
            {
                return Unauthorized(new { ErrorMessage = "Usuario no autorizado.", ErrorCode = "UNAUTHORIZED" });
            }

            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(new
                {
                    ErrorMessage = "Errores de validación en la solicitud.",
                    ErrorCode = "VALIDATION_ERROR",
                    Errors = errors
                });
            }

            var result = await configuracionService.GuardarAsync(usuarioId, dto);

            if (!result.IsSuccess)
            {
                return BadRequest(new { result.ErrorMessage, result.ErrorCode });
            }

            return NoContent();
        }
    }
}
