using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.UnidadesMedida;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Application.Mappings;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Services
{
    public class UnidadMedidaService(IRepository<UnidadMedida, int> repository) : IUnidadMedidaService
    {
        private readonly IRepository<UnidadMedida, int> _repository = repository;

        public async Task<Result<IEnumerable<UnidadMedidaDto>>> GetAllAsync()
        {
            var unidades = await _repository.GetAllAsync();
            var dtos = unidades.Select(u => u.ToDto());
            return Result<IEnumerable<UnidadMedidaDto>>.Success(dtos);
        }
    }
}
