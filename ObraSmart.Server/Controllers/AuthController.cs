using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ObraSmart.Application.DTOs;
using ObraSmart.Application.Interfaces;

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
        public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioDto dto)
        {
            var resultado = await _authService.RegistrarAsync(dto);

            if (!resultado.IsSuccess)
                return BadRequest(new { error = resultado.ErrorMessage, code = resultado.ErrorCode });

            return Ok(new { mensaje = "Usuario registrado exitosamente" });
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
