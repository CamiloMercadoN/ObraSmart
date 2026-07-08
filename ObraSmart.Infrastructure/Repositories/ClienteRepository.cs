using Microsoft.EntityFrameworkCore;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Infrastructure.Data;


namespace ObraSmart.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly ObraSmartDbContext _context;

        public ClienteRepository(ObraSmartDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cliente>> ObtenerTodosAsync(Guid usuarioId)
        {
            return await _context.Clientes
                .AsNoTracking()
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<Cliente?> ObtenerPorIdAsync(Guid id, Guid usuarioId)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId);
        }

        public async Task AgregarAsync(Cliente cliente)
        {
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Cliente cliente)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> TienePresupuestosAsociadosAsync(Guid id)
        {
            return await _context.Presupuestos.AnyAsync(p => p.ClienteId == id);
        }
    }
}
