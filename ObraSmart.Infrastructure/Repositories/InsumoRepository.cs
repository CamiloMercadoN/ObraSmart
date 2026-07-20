using Microsoft.EntityFrameworkCore;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Infrastructure.Data;

namespace ObraSmart.Infrastructure.Repositories
{
    public class InsumoRepository : RepositoryBase<Insumo, Guid>, IInsumoRepository
    {
        public InsumoRepository(ObraSmartDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Insumo>> GetAllWithDependenciesAsync()
        {
            return await _context.Insumos
                .AsNoTracking()
                .Include(i => i.UnidadMedida)
                .Include(i => i.Etiquetas)
                .ToListAsync();
        }

        public async Task<Insumo?> GetByIdWithDependenciesAsync(Guid id)
        {
            return await _context.Insumos
                .Include(i => i.UnidadMedida)
                .Include(i => i.Etiquetas)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Etiqueta>> GetEtiquetasByIdsAsync(IEnumerable<Guid> ids)
        {
            return await _context.Etiquetas
                .Where(e => ids.Contains(e.Id))
                .ToListAsync();
        }
        public async Task<IReadOnlyList<Insumo>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            return await _context.Insumos
                .AsNoTracking()
                .Where(i => ids.Contains(i.Id))
                .ToListAsync();
        }

    }
}
