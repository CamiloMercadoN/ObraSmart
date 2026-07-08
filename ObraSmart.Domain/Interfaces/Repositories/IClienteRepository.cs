using ObraSmart.Domain.Entities;

namespace ObraSmart.Domain.Interfaces.Repositories
{
    public interface IClienteRepository
    {
        Task<IEnumerable<Cliente>> ObtenerTodosAsync(Guid usuarioId);
        Task<Cliente?> ObtenerPorIdAsync(Guid id, Guid usuarioId);
        Task AgregarAsync(Cliente cliente);
        Task ActualizarAsync(Cliente cliente);
        Task EliminarAsync(Cliente cliente);
        Task<bool> TienePresupuestosAsociadosAsync(Guid id);
    }
}
