using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Dashboard;
using ObraSmart.Application.Interfaces.Services;
using System.Security.Claims;

namespace ObraSmart.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController(IDashboardService _dashboardService) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<Result<DashboardDto>>> Get()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(usuarioIdClaim, out Guid usuarioId))
            {
                return Unauthorized(Result<DashboardDto>.Failure("Usuario no válido.", "UNAUTHORIZED"));
            }

            var result = await _dashboardService.ObtenerResumenAsync(usuarioId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
