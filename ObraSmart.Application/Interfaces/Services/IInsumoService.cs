using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Insumos;

namespace ObraSmart.Application.Interfaces.Services
{
    public interface IInsumoService
    {
        Task<Result<IEnumerable<InsumoDto>>> GetAllAsync();
        Task<Result<InsumoDto>> GetByIdAsync(Guid id);
        Task<Result<Guid>> CreateAsync(InsumoUpsertDto dto);
        Task<Result> UpdateAsync(Guid id, InsumoUpsertDto dto);
        Task<Result> DeleteAsync(Guid id);
    }
}
