using Microsoft.EntityFrameworkCore;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Infrastructure.Data;

namespace ObraSmart.Infrastructure.Repositories
{
    public class EstructuraAPURepository : RepositoryBase<EstructuraAPU, Guid>, IEstructuraAPURepository
    {
        public EstructuraAPURepository(ObraSmartDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<EstructuraAPU>> GetAllWithDependenciesAsync()
        {
            return await _context.Set<EstructuraAPU>()
                .AsNoTracking()
                .Include(a => a.UnidadMedida)
                .Include(a => a.Etiquetas)
                .Include(a => a.Componentes)
                    .ThenInclude(c => c.Insumo)
                .ToListAsync();
        }

        public async Task<EstructuraAPU?> GetByIdWithDependenciesAsync(Guid id)
        {
            return await _context.Set<EstructuraAPU>()
                .Include(a => a.UnidadMedida)
                .Include(a => a.Etiquetas)
                .Include(a => a.Componentes)
                    .ThenInclude(c => c.Insumo)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateGrafoAsync(EstructuraAPU apu, IEnumerable<ComponenteAPU> componentesAEliminar)
        {
            // Eliminamos físicamente los insumos que fueron quitados de la receta
            if (componentesAEliminar.Any())
            {
                _context.Set<ComponenteAPU>().RemoveRange(componentesAEliminar);
            }

            // Actualizamos la entidad principal (EF Core detectará los nuevos y modificados)
            _context.Set<EstructuraAPU>().Update(apu);

            // Guardamos los cambios en una sola transacción a SQL Server
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGrafoAsync(EstructuraAPU apu)
        {
            // Eliminar explícitamente los hijos (Componentes) para respetar la restricción del DbContext
            if (apu.Componentes != null && apu.Componentes.Any())
            {
                _context.Set<ComponenteAPU>().RemoveRange(apu.Componentes);
            }

            // Limpiar las etiquetas
            if (apu.Etiquetas != null)
            {
                apu.Etiquetas.Clear();
            }

            // Eliminar el padre
            _context.Set<EstructuraAPU>().Remove(apu);

            // Confirmar todo en una sola transacción
            await _context.SaveChangesAsync();
        }
    }
}
