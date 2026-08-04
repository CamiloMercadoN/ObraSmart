using ObraSmart.Domain.Entities;

namespace ObraSmart.Domain.Interfaces.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario, Guid>
    {
        Task<Usuario?> ObtenerPorCorreoAsync(string correo);
    }
}
