using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Territorios;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Services
{
    public class TerritorioService : ITerritorioService
    {
        private readonly ITerritorioRepository _territorioRepository;

        public TerritorioService(ITerritorioRepository territorioRepository)
        {
            _territorioRepository = territorioRepository;
        }

        public async Task<Result<IEnumerable<TerritorioDto>>> ObtenerRegionesAsync()
        {
            var regiones = await _territorioRepository.ObtenerRegionesAsync();
            var dtos = regiones.Select(r => new TerritorioDto { Id = r.Id, Nombre = r.Nombre });
            return Result<IEnumerable<TerritorioDto>>.Success(dtos);
        }

        public async Task<Result<IEnumerable<TerritorioDto>>> ObtenerCiudadesPorRegionAsync(int regionId)
        {
            var ciudades = await _territorioRepository.ObtenerCiudadesPorRegionAsync(regionId);
            var dtos = ciudades.Select(c => new TerritorioDto { Id = c.Id, Nombre = c.Nombre });
            return Result<IEnumerable<TerritorioDto>>.Success(dtos);
        }
    }
}
