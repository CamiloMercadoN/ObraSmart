using ObraSmart.Domain.Entities;

namespace ObraSmart.Domain.Interfaces.Repositories
{
    public interface IInsumoRepository : IRepository<Insumo, Guid>
    {
        Task<IReadOnlyList<Insumo>> GetAllWithDependenciesAsync();
        Task<Insumo?> GetByIdWithDependenciesAsync(Guid id);
        Task<List<Etiqueta>> GetEtiquetasByIdsAsync(IEnumerable<Guid> ids);
    }
}
