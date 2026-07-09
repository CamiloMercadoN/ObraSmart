using ObraSmart.Domain.Entities;

namespace ObraSmart.Domain.Interfaces.Repositories
{
    public interface ITerritorioRepository
    {
        Task<IEnumerable<EstadoProvincia>> ObtenerRegionesAsync();
        Task<IEnumerable<Ciudad>> ObtenerCiudadesPorRegionAsync(int regionId);
    }
}
