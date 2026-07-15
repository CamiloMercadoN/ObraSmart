using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObraSmart.Application.DTOs.Etiquetas;
using ObraSmart.Application.Interfaces.Services;

namespace ObraSmart.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EtiquetasController(
        IEtiquetaService service,
        IValidator<EtiquetaUpsertDto> validator) : ControllerBase
    {
        private readonly IEtiquetaService _service = service;
        private readonly IValidator<EtiquetaUpsertDto> _validator = validator;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EtiquetaDto>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] EtiquetaUpsertDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Error = validationResult.Errors.First().ErrorMessage });
            }

            var result = await _service.CreateAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return Ok(result.Data);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] EtiquetaUpsertDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Error = validationResult.Errors.First().ErrorMessage });
            }

            var result = await _service.UpdateAsync(id, dto);
            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return NoContent();
        }
    }
}
