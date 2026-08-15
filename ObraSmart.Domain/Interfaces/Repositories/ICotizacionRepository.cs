using ObraSmart.Domain.Entities;

namespace ObraSmart.Domain.Interfaces.Repositories
{
    public interface ICotizacionRepository : IRepository<Cotizacion, Guid>
    {
        Task<Cotizacion?> GetByIdWithDependenciesAsync(Guid id);
        Task<IReadOnlyList<Cotizacion>> GetAllWithDependenciesByUsuarioAsync(Guid usuarioId);
    }
}
