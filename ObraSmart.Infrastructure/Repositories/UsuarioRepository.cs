using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ObraSmart.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ObraSmartDbContext _context;

        public UsuarioRepository(ObraSmartDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
        }

        public async Task AgregarAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
