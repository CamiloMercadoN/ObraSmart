using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Presupuestos;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Application.Mappings;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Services
{
    public class PresupuestoService(
        IPresupuestoRepository presupuestoRepository,
        IRepository<Usuario, Guid> usuarioRepository) : IPresupuestoService
    {
        private readonly IPresupuestoRepository _presupuestoRepository = presupuestoRepository;
        private readonly IRepository<Usuario, Guid> _usuarioRepository = usuarioRepository;

        public async Task<Result<IEnumerable<PresupuestoDto>>> ObtenerTodosAsync()
        {
            var presupuestos = await _presupuestoRepository.GetAllWithDependenciesAsync();

            var dtos = presupuestos.Select(p => p.ToDto());

            return Result<IEnumerable<PresupuestoDto>>.Success(dtos);
        }

        public async Task<Result<PresupuestoDto>> ObtenerPorIdAsync(Guid id)
        {
            var presupuesto = await _presupuestoRepository.GetByIdWithDependenciesAsync(id);

            if (presupuesto == null)
                return Result<PresupuestoDto>.Failure("El presupuesto no fue encontrado.", "NOT_FOUND");

            return Result<PresupuestoDto>.Success(presupuesto.ToDto());
        }

        public async Task<Result<Guid>> CrearAsync(PresupuestoUpsertDto dto, Guid usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null) return Result<Guid>.Failure("Usuario no encontrado", "USER_NOT_FOUND");

            var presupuesto = dto.ToEntity();
            presupuesto.UsuarioId = usuarioId;
            presupuesto.FechaCreacion = DateTime.UtcNow;
            presupuesto.Estado = "Borrador";
            presupuesto.EsPlantilla = false;

            RecalcularTotalesPresupuesto(presupuesto, usuario.PorcentajeIva);

            await _presupuestoRepository.AddAsync(presupuesto);

            return Result<Guid>.Success(presupuesto.Id);
        }

        public async Task<Result> ActualizarAsync(Guid id, PresupuestoUpsertDto dto, Guid usuarioId)
        {
            var presupuesto = await _presupuestoRepository.GetByIdWithDependenciesAsync(id);
            if (presupuesto == null)
                return Result.Failure("Presupuesto no encontrado.", "NOT_FOUND");

            if (presupuesto.Estado != "Borrador")
                return Result.Failure("Solo se pueden editar presupuestos en estado Borrador.", "INVALID_STATE");

            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);

            presupuesto.ClienteId = dto.ClienteId;
            presupuesto.NombreProyecto = dto.NombreProyecto;

            var itemsAEliminar = new List<ItemPresupuesto>();
            var recursosAEliminar = new List<RecursoItemPresupuesto>();

            // Identificar Items a eliminar (Están en BD pero ya no en DTO)
            var dtoItemIds = dto.Items.Where(i => i.Id.HasValue).Select(i => i.Id).ToList();
            var itemsParaBorrar = presupuesto.Items.Where(i => !dtoItemIds.Contains(i.Id)).ToList();

            foreach (var item in itemsParaBorrar)
            {
                // Agregamos todos sus recursos para cumplir con DeleteBehavior.Restrict
                recursosAEliminar.AddRange(item.Recursos);
                itemsAEliminar.Add(item);
                presupuesto.Items.Remove(item);
            }

            // Sincronizar Items existentes y agregar nuevos
            foreach (var itemDto in dto.Items)
            {
                if (itemDto.Id.HasValue && itemDto.Id.Value != Guid.Empty)
                {
                    var itemExistente = presupuesto.Items.FirstOrDefault(i => i.Id == itemDto.Id.Value);
                    if (itemExistente != null)
                    {
                        itemExistente.Descripcion = itemDto.Descripcion;
                        itemExistente.CantidadItem = itemDto.CantidadItem;
                        itemExistente.UnidadMedidaId = itemDto.UnidadMedidaId;

                        // Sincronizar Recursos del Item
                        var dtoRecursoIds = itemDto.Recursos.Where(r => r.Id.HasValue).Select(r => r.Id).ToList();
                        var recursosParaBorrar = itemExistente.Recursos.Where(r => !dtoRecursoIds.Contains(r.Id)).ToList();

                        foreach (var rec in recursosParaBorrar)
                        {
                            recursosAEliminar.Add(rec);
                            itemExistente.Recursos.Remove(rec);
                        }

                        foreach (var recDto in itemDto.Recursos)
                        {
                            if (recDto.Id.HasValue && recDto.Id.Value != Guid.Empty)
                            {
                                var recExistente = itemExistente.Recursos.FirstOrDefault(r => r.Id == recDto.Id.Value);
                                if (recExistente != null)
                                {
                                    recExistente.Cantidad = recDto.Cantidad;
                                    recExistente.PrecioUnitarioCongelado = recDto.PrecioUnitarioCongelado;
                                    recExistente.DescripcionCongelada = recDto.DescripcionCongelada;
                                    recExistente.UnidadMedidaId = recDto.UnidadMedidaId;
                                }
                            }
                            else
                            {
                                itemExistente.Recursos.Add(new RecursoItemPresupuesto
                                {
                                    TipoInsumo = recDto.TipoInsumo,
                                    DescripcionCongelada = recDto.DescripcionCongelada,
                                    Cantidad = recDto.Cantidad,
                                    PrecioUnitarioCongelado = recDto.PrecioUnitarioCongelado,
                                    UnidadMedidaId = recDto.UnidadMedidaId
                                });
                            }
                        }
                    }
                }
                else
                {
                    // Es un Item completamente nuevo
                    var nuevoItem = new ItemPresupuesto
                    {
                        EstructuraAPUOrigenId = itemDto.EstructuraAPUOrigenId,
                        Descripcion = itemDto.Descripcion,
                        CantidadItem = itemDto.CantidadItem,
                        UnidadMedidaId = itemDto.UnidadMedidaId,
                        Recursos = itemDto.Recursos.Select(r => new RecursoItemPresupuesto
                        {
                            TipoInsumo = r.TipoInsumo,
                            DescripcionCongelada = r.DescripcionCongelada,
                            Cantidad = r.Cantidad,
                            PrecioUnitarioCongelado = r.PrecioUnitarioCongelado,
                            UnidadMedidaId = r.UnidadMedidaId
                        }).ToList()
                    };
                    presupuesto.Items.Add(nuevoItem);
                }
            }

            // Recalcular matemáticamente la nueva estructura
            RecalcularTotalesPresupuesto(presupuesto, usuario!.PorcentajeIva);

            // Guardar pasando las listas de eliminación manual
            await _presupuestoRepository.UpdateGrafoAsync(presupuesto, itemsAEliminar, recursosAEliminar);

            return Result.Success();
        }

        public async Task<Result> EliminarAsync(Guid id)
        {
            var presupuesto = await _presupuestoRepository.GetByIdWithDependenciesAsync(id);
            if (presupuesto == null)
                return Result.Failure("Presupuesto no encontrado.", "NOT_FOUND");

            if (presupuesto.Estado != "Borrador")
                return Result.Failure("No se puede eliminar un presupuesto que ya ha sido procesado o emitido.", "INVALID_STATE");

            await _presupuestoRepository.DeleteGrafoAsync(presupuesto);

            return Result.Success();
        }

        /// <summary>
        /// Recalcula el presupuesto completo desde el nivel de recursos hacia arriba.
        /// </summary>
        private void RecalcularTotalesPresupuesto(Presupuesto presupuesto, decimal porcentajeIva)
        {
            presupuesto.Subtotal = 0;

            foreach (var item in presupuesto.Items)
            {
                decimal precioUnitarioItem = 0;

                foreach (var recurso in item.Recursos)
                {
                    // Costo del recurso = Cantidad de Insumo * Precio Congelado
                    recurso.CostoTotalRecurso = Math.Round(recurso.Cantidad * recurso.PrecioUnitarioCongelado, 2);
                    precioUnitarioItem += recurso.CostoTotalRecurso;
                }

                // Costos del ítem consolidado
                item.PrecioUnitarioCalculado = Math.Round(precioUnitarioItem, 2);
                item.Subtotal = Math.Round(item.CantidadItem * item.PrecioUnitarioCalculado, 2);

                // Sumar al presupuesto general
                presupuesto.Subtotal += item.Subtotal;
            }

            // Totales finales del Presupuesto
            presupuesto.Subtotal = Math.Round(presupuesto.Subtotal, 2);
            presupuesto.MontoIva = Math.Round(presupuesto.Subtotal * (porcentajeIva / 100m), 2);
            presupuesto.Total = presupuesto.Subtotal + presupuesto.MontoIva;
        }
    }
}
