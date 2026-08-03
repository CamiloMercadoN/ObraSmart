using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ObraSmart.Application.DTOs;
using ObraSmart.Application.Interfaces.Services;

namespace ObraSmart.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("registro")]
        public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioDto dto, [FromServices] IValidator<RegistroUsuarioDto> validator)
        {
            ValidationResult validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var primerError = validationResult.Errors.First().ErrorMessage;
                return BadRequest(new { error = primerError, code = "FORMATO_INVALIDO" });
            }

            var resultado = await _authService.RegistrarAsync(dto);

            if (!resultado.IsSuccess)
            {
                if (resultado.ErrorCode == "EMAIL_DUPLICADO")
                {
                    return Conflict(new { error = resultado.ErrorMessage, code = resultado.ErrorCode });
                }

                return BadRequest(new { error = resultado.ErrorMessage, code = resultado.ErrorCode });
            }

            return Created("", new { mensaje = "Usuario registrado exitosamente" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var resultado = await _authService.LoginAsync(dto);

            if (!resultado.IsSuccess)
                return Unauthorized(new { error = resultado.ErrorMessage, code = resultado.ErrorCode });

            return Ok(new { token = resultado.Data });
        }
    }
}
