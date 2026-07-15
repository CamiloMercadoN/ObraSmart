using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObraSmart.Application.DTOs.UnidadesMedida;
using ObraSmart.Application.Interfaces.Services;

namespace ObraSmart.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UnidadesMedidaController(IUnidadMedidaService service) : ControllerBase
    {
        private readonly IUnidadMedidaService _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnidadMedidaDto>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }
            return Ok(result.Data);
        }
    }
}
