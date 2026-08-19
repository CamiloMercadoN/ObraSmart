using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Dashboard;

namespace ObraSmart.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<Result<DashboardDto>> ObtenerResumenAsync(Guid usuarioId);
    }
}
