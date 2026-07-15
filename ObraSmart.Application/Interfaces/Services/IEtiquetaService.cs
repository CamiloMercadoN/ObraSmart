using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Etiquetas;

namespace ObraSmart.Application.Interfaces.Services
{
    public interface IEtiquetaService
    {
        Task<Result<IEnumerable<EtiquetaDto>>> GetAllAsync();
        Task<Result<Guid>> CreateAsync(EtiquetaUpsertDto dto);
        Task<Result> UpdateAsync(Guid id, EtiquetaUpsertDto dto);
        Task<Result> DeleteAsync(Guid id);
    }
}
