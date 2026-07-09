using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Territorios;

namespace ObraSmart.Application.Interfaces
{
    public interface ITerritorioService
    {
        Task<Result<IEnumerable<TerritorioDto>>> ObtenerRegionesAsync();
        Task<Result<IEnumerable<TerritorioDto>>> ObtenerCiudadesPorRegionAsync(int regionId);
    }
}
