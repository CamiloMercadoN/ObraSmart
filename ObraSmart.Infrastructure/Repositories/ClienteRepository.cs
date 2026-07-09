using Microsoft.EntityFrameworkCore;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Infrastructure.Data;


namespace ObraSmart.Infrastructure.Repositories
{
    public class ClienteRepository(ObraSmartDbContext context) : RepositoryBase<Cliente, Guid>(context), IClienteRepository
    {
        public override async Task<IReadOnlyList<Cliente>> GetAllAsync()
        {
            return await _context.Clientes
                            .Include(c => c.Ciudad)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public override async Task<Cliente?> GetByIdAsync(Guid id)
        {
            return await _context.Clientes
                            .Include(c => c.Ciudad)
                            .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> TienePresupuestosAsociadosAsync(Guid id)
        {
            return await _context.Presupuestos.AnyAsync(p => p.ClienteId == id);
        }
    }
}
