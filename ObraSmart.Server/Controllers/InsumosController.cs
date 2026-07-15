using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObraSmart.Application.DTOs.Insumos;
using ObraSmart.Application.Interfaces.Services;

namespace ObraSmart.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InsumosController : ControllerBase
    {
        private readonly IInsumoService _insumoService;
        private readonly IValidator<InsumoUpsertDto> _validator;

        public InsumosController(
            IInsumoService insumoService,
            IValidator<InsumoUpsertDto> validator)
        {
            _insumoService = insumoService;
            _validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InsumoDto>>> GetAll()
        {
            var result = await _insumoService.GetAllAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return Ok(result.Data);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<InsumoDto>> GetById(Guid id)
        {
            var result = await _insumoService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] InsumoUpsertDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Error = validationResult.Errors.First().ErrorMessage });
            }

            var result = await _insumoService.CreateAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] InsumoUpsertDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Error = validationResult.Errors.First().ErrorMessage });
            }

            var result = await _insumoService.UpdateAsync(id, dto);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _insumoService.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return NoContent();
        }
    }
}
