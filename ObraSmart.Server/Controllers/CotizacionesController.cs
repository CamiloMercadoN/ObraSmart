using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ObraSmart.Application.DTOs.Cotizaciones;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Application.Mappings;

namespace ObraSmart.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CotizacionesController(
        ICotizacionService _cotizacionService,
        IValidator<CrearCotizacionRequestDto> _crearValidator,
        IValidator<ActualizarEstadoCotizacionRequestDto> _estadoValidator,
        IValidator<RenovarVigenciaCotizacionRequestDto> _vigenciaValidator) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CotizacionResponseDto>>> ObtenerTodas()
        {
            var result = await _cotizacionService.ObtenerTodasPorUsuarioAsync();
            if (!result.IsSuccess) return Unauthorized();

            var dtos = result.Data!.Select(c => c.ToResponseDto());
            return Ok(dtos);
        }

        [HttpPost]
        public async Task<ActionResult<CotizacionResponseDto>> CrearCotizacion([FromBody] CrearCotizacionRequestDto request)
        {
            var validationResult = await _crearValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var result = await _cotizacionService.CrearCotizacionAsync(request);

            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "UNAUTHORIZED") return Unauthorized();
                if (result.ErrorCode == "NOT_FOUND") return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            var responseDto = result.Data!.ToResponseDto();
            return CreatedAtAction(nameof(CrearCotizacion), new { id = responseDto.Id }, responseDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CotizacionResponseDto>> ObtenerPorId(Guid id)
        {
            var result = await _cotizacionService.ObtenerCotizacionPorIdAsync(id);

            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "UNAUTHORIZED") return Unauthorized();
                if (result.ErrorCode == "NOT_FOUND") return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data!.ToResponseDto());
        }

        [HttpPatch("{id}/estado")]
        public async Task<ActionResult<CotizacionResponseDto>> ActualizarEstado(Guid id, [FromBody] ActualizarEstadoCotizacionRequestDto request)
        {
            var validationResult = await _estadoValidator.ValidateAsync(request);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            var result = await _cotizacionService.ActualizarEstadoAsync(id, request);

            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "UNAUTHORIZED") return Unauthorized();
                if (result.ErrorCode == "NOT_FOUND") return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data!.ToResponseDto());
        }

        [HttpPatch("{id}/vigencia")]
        public async Task<ActionResult<CotizacionResponseDto>> RenovarVigencia(Guid id, [FromBody] RenovarVigenciaCotizacionRequestDto request)
        {
            var validationResult = await _vigenciaValidator.ValidateAsync(request);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            var result = await _cotizacionService.RenovarVigenciaAsync(id, request);

            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "UNAUTHORIZED") return Unauthorized();
                if (result.ErrorCode == "NOT_FOUND") return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data!.ToResponseDto());
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DescargarPdf(Guid id)
        {
            var result = await _cotizacionService.ExportarPdfAsync(id);

            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "UNAUTHORIZED") return Unauthorized();
                if (result.ErrorCode == "NOT_FOUND") return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            var cotizacionResult = await _cotizacionService.ObtenerCotizacionPorIdAsync(id);
            var numeroCotizacion = cotizacionResult.IsSuccess ? cotizacionResult.Data!.NumeroCotizacion : id.ToString();

            return File(result.Data!, "application/pdf", $"Cotizacion-{numeroCotizacion}.pdf");
        }
    }
}
