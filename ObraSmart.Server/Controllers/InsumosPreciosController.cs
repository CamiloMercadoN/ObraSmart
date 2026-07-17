using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObraSmart.Application.DTOs.Insumos;
using ObraSmart.Application.Interfaces.Services;

namespace ObraSmart.Server.Controllers
{
    [Route("api/insumos/precios")]
    [ApiController]
    [Authorize]
    public class InsumosPreciosController : ControllerBase
    {
        private readonly IInsumoPrecioService _precioService;
        private readonly IValidator<ActualizarPrecioDto> _precioValidator;
        private readonly IValidator<ReajusteLoteDto> _reajusteValidator;

        public InsumosPreciosController(
            IInsumoPrecioService precioService,
            IValidator<ActualizarPrecioDto> precioValidator,
            IValidator<ReajusteLoteDto> reajusteValidator)
        {
            _precioService = precioService;
            _precioValidator = precioValidator;
            _reajusteValidator = reajusteValidator;
        }

        //Actualización Individual Directa (PATCH)
        [HttpPatch("{id:guid}")]
        public async Task<ActionResult> ActualizarPrecio(Guid id, [FromBody] ActualizarPrecioDto dto)
        {
            var validationResult = await _precioValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Error = validationResult.Errors.First().ErrorMessage });
            }

            var result = await _precioService.ActualizarPrecioIndividualAsync(id, dto);
            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return NoContent();
        }

        // Reajuste por Lote mediante Filtros (POST)
        [HttpPost("reajuste-lote")]
        public async Task<ActionResult<ResumenProcesamientoDto>> ReajustarPorLote([FromBody] ReajusteLoteDto dto)
        {
            var validationResult = await _reajusteValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Error = validationResult.Errors.First().ErrorMessage });
            }

            var result = await _precioService.ReajustarPreciosLoteAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return Ok(result.Data);
        }

        //Importación por Archivo CSV (POST Multipart Form Data)
        [HttpPost("importar-csv")]
        public async Task<ActionResult<ResumenProcesamientoDto>> ImportarCsv(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new { Error = "Debe cargar un archivo CSV válido." });
            }

            if (!Path.GetExtension(archivo.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { Error = "El formato de archivo es inválido. Solo se admiten archivos .csv" });
            }

            using var stream = archivo.OpenReadStream();
            var result = await _precioService.ImportarPreciosCsvAsync(stream);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return Ok(result.Data);
        }

        [HttpGet("exportar-plantilla")]
        public async Task<IActionResult> ExportarPlantilla()
        {
            var result = await _precioService.ExportarPlantillaCsvAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
            }

            return File(result.Data, "text/csv", "Plantilla_Actualizacion_Precios.csv");
        }
    }
}
