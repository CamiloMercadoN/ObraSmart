using ObraSmart.Domain.Entities;

namespace ObraSmart.Domain.Interfaces.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorCorreoAsync(string correo);
        Task AgregarAsync(Usuario usuario);
        Task GuardarCambiosAsync();
    }
}
