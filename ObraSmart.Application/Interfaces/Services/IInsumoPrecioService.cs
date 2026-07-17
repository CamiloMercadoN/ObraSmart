using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Insumos;

namespace ObraSmart.Application.Interfaces.Services
{
    public interface IInsumoPrecioService
    {
        Task<Result> ActualizarPrecioIndividualAsync(Guid id, ActualizarPrecioDto dto);
        Task<Result<ResumenProcesamientoDto>> ReajustarPreciosLoteAsync(ReajusteLoteDto dto);
        Task<Result<ResumenProcesamientoDto>> ImportarPreciosCsvAsync(Stream fileStream);
        Task<Result<byte[]>> ExportarPlantillaCsvAsync();
    }
}
