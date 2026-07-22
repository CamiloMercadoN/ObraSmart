using Microsoft.EntityFrameworkCore;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Infrastructure.Data;

namespace ObraSmart.Infrastructure.Repositories
{
    public class PresupuestoRepository : RepositoryBase<Presupuesto, Guid>, IPresupuestoRepository
    {
        public PresupuestoRepository(ObraSmartDbContext context) : base(context) { }

        public async Task<IReadOnlyList<Presupuesto>> GetAllWithDependenciesAsync()
        {
            return await _context.Set<Presupuesto>()
                .Include(p => p.Cliente)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Presupuesto?> GetByIdWithDependenciesAsync(Guid id)
        {
            return await _context.Set<Presupuesto>()
                            .Include(p => p.Cliente)
                            .Include(p => p.Items)
                                .ThenInclude(i => i.Recursos)
                            .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task UpdateGrafoAsync(Presupuesto presupuesto, IEnumerable<ItemPresupuesto> itemsAEliminar, IEnumerable<RecursoItemPresupuesto> recursosAEliminar)
        {
            // OBLIGATORIO: Borrar primero el nivel 3 (Recursos)
            if (recursosAEliminar.Any())
                _context.Set<RecursoItemPresupuesto>().RemoveRange(recursosAEliminar);

            // OBLIGATORIO: Borrar luego el nivel 2 (Ítems)
            if (itemsAEliminar.Any())
                _context.Set<ItemPresupuesto>().RemoveRange(itemsAEliminar);

            // Actualizar Cabecera (El ChangeTracker de EF Core detectará INSERTS y UPDATES automáticamente)
            _context.Set<Presupuesto>().Update(presupuesto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGrafoAsync(Presupuesto presupuesto)
        {
            // Borrado manual en cascada Inversa
            foreach (var item in presupuesto.Items)
            {
                if (item.Recursos.Any())
                    _context.Set<RecursoItemPresupuesto>().RemoveRange(item.Recursos);
            }

            if (presupuesto.Items.Any())
                _context.Set<ItemPresupuesto>().RemoveRange(presupuesto.Items);

            _context.Set<Presupuesto>().Remove(presupuesto);
            await _context.SaveChangesAsync();
        }
    }
}
