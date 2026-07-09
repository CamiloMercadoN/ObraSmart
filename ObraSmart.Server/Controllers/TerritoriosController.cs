using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObraSmart.Application.Interfaces;

namespace ObraSmart.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TerritoriosController : ControllerBase
    {
        private readonly ITerritorioService _territorioService;

        public TerritoriosController(ITerritorioService territorioService)
        {
            _territorioService = territorioService;
        }

        [HttpGet("regiones")]
        public async Task<IActionResult> ObtenerRegiones()
        {
            var result = await _territorioService.ObtenerRegionesAsync();
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpGet("regiones/{regionId:int}/ciudades")]
        public async Task<IActionResult> ObtenerCiudades(int regionId)
        {
            var result = await _territorioService.ObtenerCiudadesPorRegionAsync(regionId);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }
    }
}
