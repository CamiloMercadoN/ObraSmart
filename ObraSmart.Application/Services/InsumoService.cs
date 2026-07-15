using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Insumos;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Application.Mappings;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Services
{
    public class InsumoService : IInsumoService
    {
        private readonly IInsumoRepository _insumoRepository;

        public InsumoService(
            IInsumoRepository insumoRepository)
        {
            _insumoRepository = insumoRepository;
        }

        public async Task<Result<IEnumerable<InsumoDto>>> GetAllAsync()
        {
            var insumos = await _insumoRepository.GetAllWithDependenciesAsync();

            var dtos = insumos.Select(i => i.ToDto());

            return Result<IEnumerable<InsumoDto>>.Success(dtos);
        }

        public async Task<Result<InsumoDto>> GetByIdAsync(Guid id)
        {
            var insumo = await _insumoRepository.GetByIdWithDependenciesAsync(id);
            if (insumo == null)
                return Result<InsumoDto>.Failure("Insumo no encontrado.");

            return Result<InsumoDto>.Success(insumo.ToDto());
        }

        public async Task<Result<Guid>> CreateAsync(InsumoUpsertDto dto)
        {

            var nuevoInsumo = dto.ToEntity();

            // Asociación de las etiquetas deseadas
            if (dto.EtiquetasIds.Count != 0)
            {
                var etiquetas = await _insumoRepository.GetEtiquetasByIdsAsync(dto.EtiquetasIds);
                foreach (var tag in etiquetas)
                {
                    nuevoInsumo.Etiquetas.Add(tag);
                }
            }

            await _insumoRepository.AddAsync(nuevoInsumo);
            return Result<Guid>.Success(nuevoInsumo.Id);
        }

        public async Task<Result> UpdateAsync(Guid id, InsumoUpsertDto dto)
        {
            var insumoDb = await _insumoRepository.GetByIdWithDependenciesAsync(id);
            if (insumoDb == null)
                return Result.Failure("Insumo no encontrado.");

            if (insumoDb.EsPlantilla)
                return Result.Failure("No se pueden modificar los insumos de plantilla global.");

            // Actualización de datos planos usando el mapper
            dto.UpdateEntity(insumoDb);

            // Sincronización limpia de la tabla intermedia Many-to-Many
            insumoDb.Etiquetas.Clear();
            if (dto.EtiquetasIds.Any())
            {
                var nuevasEtiquetas = await _insumoRepository.GetEtiquetasByIdsAsync(dto.EtiquetasIds);
                foreach (var tag in nuevasEtiquetas)
                {
                    insumoDb.Etiquetas.Add(tag);
                }
            }

            await _insumoRepository.UpdateAsync(insumoDb);
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var insumoDb = await _insumoRepository.GetByIdAsync(id);
            if (insumoDb == null)
                return Result.Failure("Insumo no encontrado.");

            if (insumoDb.EsPlantilla)
                return Result.Failure("No se pueden eliminar los insumos de plantilla global.");

            await _insumoRepository.DeleteAsync(insumoDb);
            return Result.Success();
        }
    }
}
