using Microsoft.EntityFrameworkCore;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Infrastructure.Data;

namespace ObraSmart.Infrastructure.Repositories
{
    public class CotizacionRepository(ObraSmartDbContext context)
            : RepositoryBase<Cotizacion, Guid>(context), ICotizacionRepository
    {
        public async Task<Cotizacion?> GetByIdWithDependenciesAsync(Guid id)
        {
            return await _context.Set<Cotizacion>()
                .Include(c => c.Presupuesto)
                    .ThenInclude(p => p!.Cliente)
                .Include(c => c.Presupuesto)
                    .ThenInclude(p => p!.Usuario)
                .Include(c => c.Presupuesto)
                    .ThenInclude(p => p!.Items)
                        .ThenInclude(i => i.Recursos)
                            .ThenInclude(r => r.UnidadMedida)
                .Include(c => c.Presupuesto)
                    .ThenInclude(p => p!.Items)
                        .ThenInclude(i => i.UnidadMedida)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IReadOnlyList<Cotizacion>> GetAllWithDependenciesByUsuarioAsync(Guid usuarioId)
        {
            return await _context.Set<Cotizacion>()
                .Include(c => c.Presupuesto)
                    .ThenInclude(p => p!.Cliente)
                .Where(c => c.Presupuesto != null && c.Presupuesto.UsuarioId == usuarioId)
                .OrderByDescending(c => c.FechaEmision)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
