using Microsoft.EntityFrameworkCore;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Infrastructure.Data;

namespace ObraSmart.Infrastructure.Repositories
{
    public class TerritorioRepository : ITerritorioRepository
    {
        private readonly ObraSmartDbContext _context;

        public TerritorioRepository(ObraSmartDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EstadoProvincia>> ObtenerRegionesAsync()
        {
            return await _context.EstadoProvincias
                .AsNoTracking()
                .OrderBy(r => r.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ciudad>> ObtenerCiudadesPorRegionAsync(int regionId)
        {
            return await _context.Ciudades
                .AsNoTracking()
                .Where(c => c.EstadoProvinciaId == regionId)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }
    }
}
