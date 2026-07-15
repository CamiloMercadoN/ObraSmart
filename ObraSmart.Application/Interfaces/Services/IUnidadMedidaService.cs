using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.UnidadesMedida;

namespace ObraSmart.Application.Interfaces.Services
{
    public interface IUnidadMedidaService
    {
        Task<Result<IEnumerable<UnidadMedidaDto>>> GetAllAsync();
    }
}
