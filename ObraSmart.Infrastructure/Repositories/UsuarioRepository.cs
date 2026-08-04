using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ObraSmart.Infrastructure.Repositories
{
    public class UsuarioRepository(ObraSmartDbContext context) : RepositoryBase<Usuario, Guid>(context), IUsuarioRepository
    {
        public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
        }
    }
}
