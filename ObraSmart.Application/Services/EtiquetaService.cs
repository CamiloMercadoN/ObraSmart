using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Etiquetas;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Application.Mappings;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Services
{
    public class EtiquetaService : IEtiquetaService
    {
        private readonly IRepository<Etiqueta, Guid> _repository;

        public EtiquetaService(
            IRepository<Etiqueta, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result<IEnumerable<EtiquetaDto>>> GetAllAsync()
        {
            var todas = await _repository.GetAllAsync();

            var filtradas = todas
                .Select(e => e.ToDto());

            return Result<IEnumerable<EtiquetaDto>>.Success(filtradas);
        }

        public async Task<Result<Guid>> CreateAsync(EtiquetaUpsertDto dto)
        {
            var nueva = dto.ToEntity();
            await _repository.AddAsync(nueva);
            return Result<Guid>.Success(nueva.Id);
        }

        public async Task<Result> UpdateAsync(Guid id, EtiquetaUpsertDto dto)
        {
            var etiquetaDb = await _repository.GetByIdAsync(id);
            if (etiquetaDb == null)
                return Result.Failure("Etiqueta no encontrada.");

            if (etiquetaDb.EsPlantilla)
                return Result.Failure("No se pueden editar etiquetas de plantilla global.");

            dto.UpdateEntity(etiquetaDb);
            await _repository.UpdateAsync(etiquetaDb);

            return Result.Success();
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var etiquetaDb = await _repository.GetByIdAsync(id);
            if (etiquetaDb == null)
                return Result.Failure("Etiqueta no encontrada.");

            if (etiquetaDb.EsPlantilla)
                return Result.Failure("No se pueden eliminar etiquetas de plantilla global.");

            await _repository.DeleteAsync(etiquetaDb);
            return Result.Success();
        }
    }
}
