using ObraSmart.Domain.Entities;

namespace ObraSmart.Domain.Interfaces.Repositories
{
    public interface IClienteRepository : IRepository<Cliente, Guid>
    {
        Task<bool> TienePresupuestosAsociadosAsync(Guid id);
    }
}
