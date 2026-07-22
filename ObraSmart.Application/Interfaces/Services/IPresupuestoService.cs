using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Presupuestos;

namespace ObraSmart.Application.Interfaces.Services
{
    public interface IPresupuestoService
    {
        Task<Result<IEnumerable<PresupuestoDto>>> ObtenerTodosAsync();
        Task<Result<PresupuestoDto>> ObtenerPorIdAsync(Guid id);
        Task<Result<Guid>> CrearAsync(PresupuestoUpsertDto dto, Guid usuarioId);
        Task<Result> ActualizarAsync(Guid id, PresupuestoUpsertDto dto, Guid usuarioId);
        Task<Result> EliminarAsync(Guid id);
    }
}
