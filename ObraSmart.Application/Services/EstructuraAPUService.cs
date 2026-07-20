using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.APUs;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Application.Mappings;

namespace ObraSmart.Application.Services
{
    public class EstructuraAPUService : IEstructuraAPUService
    {
        private readonly IEstructuraAPURepository _apuRepository;
        private readonly IInsumoRepository _insumoRepository;
        private readonly IRepository<Etiqueta, Guid> _etiquetaRepository;

        public EstructuraAPUService(
            IEstructuraAPURepository apuRepository,
            IInsumoRepository insumoRepository,
            IRepository<Etiqueta, Guid> etiquetaRepository)
        {
            _apuRepository = apuRepository;
            _insumoRepository = insumoRepository;
            _etiquetaRepository = etiquetaRepository;
        }

        public async Task<Result<IEnumerable<EstructuraAPUDto>>> ObtenerTodosAsync()
        {
            var apus = await _apuRepository.GetAllWithDependenciesAsync();

            var dtos = apus.Select(a => a.ToDto()).ToList();
            return Result<IEnumerable<EstructuraAPUDto>>.Success(dtos);
        }

        public async Task<Result<EstructuraAPUDto>> ObtenerPorIdAsync(Guid id)
        {

            var apu = await _apuRepository.GetByIdWithDependenciesAsync(id);

            if (apu == null)
                return Result<EstructuraAPUDto>.Failure("La estructura APU no fue encontrada.");

            return Result<EstructuraAPUDto>.Success(apu.ToDto());
        }

        public async Task<Result<Guid>> CrearAsync(EstructuraAPUUpsertDto dto)
        {
            var insumosIds = dto.Componentes.Select(c => c.InsumoId).Distinct().ToList();
            var insumosActuales = await _insumoRepository.GetByIdsAsync(insumosIds);

            if (insumosActuales.Count() != insumosIds.Count)
                return Result<Guid>.Failure("Uno o más insumos proporcionados no existen en el catálogo.");

            var etiquetas = new List<Etiqueta>();
            if (dto.EtiquetasIds.Count != 0)
            {
                foreach (var etiquetaId in dto.EtiquetasIds.Distinct())
                {
                    var etiquetaTracked = await _etiquetaRepository.GetByIdAsync(etiquetaId);
                    if (etiquetaTracked != null)
                    {
                        etiquetas.Add(etiquetaTracked);
                    }
                }
            }

            decimal costoTotal = 0;
            var componentes = new List<ComponenteAPU>();

            foreach (var compDto in dto.Componentes)
            {
                var insumo = insumosActuales.First(i => i.Id == compDto.InsumoId);

                costoTotal += insumo.PrecioReferencia * compDto.Cantidad;

                componentes.Add(new ComponenteAPU
                {
                    InsumoId = compDto.InsumoId,
                    Cantidad = compDto.Cantidad
                });
            }

            var nuevaApu = new EstructuraAPU
            {
                Nombre = dto.Nombre,
                UnidadMedidaId = dto.UnidadMedidaId,
                CostoTotalCalculado = Math.Round(costoTotal, 2),
                EsPlantilla = false,
                Componentes = componentes,
                Etiquetas = etiquetas
            };

            await _apuRepository.AddAsync(nuevaApu);
            return Result<Guid>.Success(nuevaApu.Id);
        }

        public async Task<Result> ActualizarAsync(Guid id, EstructuraAPUUpsertDto dto)
        {
            var apu = await _apuRepository.GetByIdWithDependenciesAsync(id);

            if (apu == null)
                return Result.Failure("La estructura APU no fue encontrada.");

            if (apu.EsPlantilla)
                return Result.Failure("No se pueden modificar las plantillas globales del sistema.");

            var insumosIds = dto.Componentes.Select(c => c.InsumoId).Distinct().ToList();
            var insumosActuales = await _insumoRepository.GetByIdsAsync(insumosIds);

            if (insumosActuales.Count() != insumosIds.Count)
                return Result.Failure("Uno o más insumos proporcionados no existen en el catálogo.");

            apu.Nombre = dto.Nombre;
            apu.UnidadMedidaId = dto.UnidadMedidaId;

            apu.Etiquetas.Clear();
            if (dto.EtiquetasIds.Count != 0)
            {
                foreach (var etiquetaId in dto.EtiquetasIds.Distinct())
                {
                    var etiquetaTracked = await _etiquetaRepository.GetByIdAsync(etiquetaId);
                    if (etiquetaTracked != null)
                    {
                        apu.Etiquetas.Add(etiquetaTracked);
                    }
                }
            }

            var componentesAEliminar = apu.Componentes
                            .Where(c => !dto.Componentes.Any(d => d.InsumoId == c.InsumoId))
                            .ToList();

            // Los quitamos de la colección en memoria
            foreach (var compEliminar in componentesAEliminar)
            {
                apu.Componentes.Remove(compEliminar);
            }

            decimal nuevoCostoTotal = 0;

            foreach (var compDto in dto.Componentes)
            {
                var insumo = insumosActuales.First(i => i.Id == compDto.InsumoId);
                nuevoCostoTotal += insumo.PrecioReferencia * compDto.Cantidad;

                var compExistente = apu.Componentes.FirstOrDefault(c => c.InsumoId == compDto.InsumoId);

                if (compExistente != null)
                {
                    // Si ya existía, solo actualizamos su cantidad
                    compExistente.Cantidad = compDto.Cantidad;
                }
                else
                {
                    // Si es nuevo, lo agregamos a la colección
                    apu.Componentes.Add(new ComponenteAPU
                    {
                        InsumoId = compDto.InsumoId,
                        Cantidad = compDto.Cantidad
                    });
                }
            }

            apu.CostoTotalCalculado = Math.Round(nuevoCostoTotal, 2);

            await _apuRepository.UpdateGrafoAsync(apu, componentesAEliminar);
            return Result.Success();
        }

        public async Task<Result> EliminarAsync(Guid id)
        {
            var apu = await _apuRepository.GetByIdWithDependenciesAsync(id);

            if (apu == null)
                return Result.Failure("La estructura APU no fue encontrada.");

            if (apu.EsPlantilla)
                return Result.Failure("No se pueden eliminar las plantillas globales del sistema.");

            await _apuRepository.DeleteGrafoAsync(apu);

            return Result.Success();
        }

        public async Task<Result> RecalcularCostoExplicitoAsync(Guid id)
        {
            var apu = await _apuRepository.GetByIdWithDependenciesAsync(id);

            if (apu == null)
                return Result.Failure("La estructura APU no existe.");

            if (apu.EsPlantilla)
                return Result.Failure("No se pueden recalcular las plantillas globales del sistema.");

            if (!apu.Componentes.Any())
                return Result.Success(); // No hay nada que recalcular

            var insumosIds = apu.Componentes.Select(c => c.InsumoId).ToList();
            var insumosActuales = await _insumoRepository.GetByIdsAsync(insumosIds);

            decimal nuevoCostoTotal = 0;

            foreach (var comp in apu.Componentes)
            {
                var insumo = insumosActuales.FirstOrDefault(i => i.Id == comp.InsumoId);
                if (insumo != null)
                {
                    nuevoCostoTotal += insumo.PrecioReferencia * comp.Cantidad;
                }
            }

            apu.CostoTotalCalculado = Math.Round(nuevoCostoTotal, 2);
            apu.Etiquetas = null!;

            await _apuRepository.UpdateAsync(apu);
            return Result.Success();
        }

    }
}