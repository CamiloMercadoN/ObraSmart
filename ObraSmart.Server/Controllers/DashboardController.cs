using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ObraSmart.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;

            return Ok(new
            {
                mensaje = "Acceso autorizado al Dashboard.",
                usuarioAuth = correo,
                id = usuarioId
            });
        }
    }
}
