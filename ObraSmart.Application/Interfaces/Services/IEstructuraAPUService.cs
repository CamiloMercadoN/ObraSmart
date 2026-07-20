using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.APUs;

namespace ObraSmart.Application.Interfaces.Services
{
    public interface IEstructuraAPUService
    {
        Task<Result<IEnumerable<EstructuraAPUDto>>> ObtenerTodosAsync();
        Task<Result<EstructuraAPUDto>> ObtenerPorIdAsync(Guid id);
        Task<Result<Guid>> CrearAsync(EstructuraAPUUpsertDto dto);
        Task<Result> ActualizarAsync(Guid id, EstructuraAPUUpsertDto dto);
        Task<Result> EliminarAsync(Guid id);
        Task<Result> RecalcularCostoExplicitoAsync(Guid id);
    }
}
