using ObraSmart.Application.DTOs.Presupuestos;
using ObraSmart.Domain.Entities;

namespace ObraSmart.Application.Mappings
{
    public static class PresupuestoMapper
    {
        public static Presupuesto ToEntity(this PresupuestoUpsertDto dto)
        {
            var presupuesto = new Presupuesto
            {
                ClienteId = dto.ClienteId,
                NombreProyecto = dto.NombreProyecto,
                Items = dto.Items.Select(i => new ItemPresupuesto
                {
                    EstructuraAPUOrigenId = i.EstructuraAPUOrigenId,
                    Descripcion = i.Descripcion,
                    CantidadItem = i.CantidadItem,
                    UnidadMedidaId = i.UnidadMedidaId,
                    Recursos = i.Recursos.Select(r => new RecursoItemPresupuesto
                    {
                        TipoInsumo = r.TipoInsumo,
                        DescripcionCongelada = r.DescripcionCongelada,
                        Cantidad = r.Cantidad,
                        PrecioUnitarioCongelado = r.PrecioUnitarioCongelado,
                        UnidadMedidaId = r.UnidadMedidaId
                    }).ToList()
                }).ToList()
            };

            return presupuesto;
        }

        public static PresupuestoDto ToDto(this Presupuesto entity)
        {
            return new PresupuestoDto
            {
                Id = entity.Id,
                ClienteId = entity.ClienteId,
                ClienteNombre = entity.Cliente != null ? entity.Cliente.Nombre : string.Empty,
                NombreProyecto = entity.NombreProyecto,
                FechaCreacion = entity.FechaCreacion,
                Estado = entity.Estado,
                Subtotal = entity.Subtotal,
                MontoIva = entity.MontoIva,
                Total = entity.Total,
                EsPlantilla = entity.EsPlantilla,
                Items = entity.Items.Select(i => new ItemPresupuestoDto
                {
                    Id = i.Id,
                    EstructuraAPUOrigenId = i.EstructuraAPUOrigenId,
                    Descripcion = i.Descripcion,
                    CantidadItem = i.CantidadItem,
                    PrecioUnitarioCalculado = i.PrecioUnitarioCalculado,
                    Subtotal = i.Subtotal,
                    UnidadMedidaId = i.UnidadMedidaId,
                    UnidadMedidaNombre = i.UnidadMedida?.Nombre ?? string.Empty,
                    Recursos = i.Recursos.Select(r => new RecursoItemPresupuestoDto
                    {
                        Id = r.Id,
                        TipoInsumo = r.TipoInsumo,
                        DescripcionCongelada = r.DescripcionCongelada,
                        Cantidad = r.Cantidad,
                        PrecioUnitarioCongelado = r.PrecioUnitarioCongelado,
                        CostoTotalRecurso = r.CostoTotalRecurso,
                        UnidadMedidaId = r.UnidadMedidaId,
                        UnidadMedidaNombre = r.UnidadMedida?.Nombre ?? string.Empty
                    }).ToList()
                }).ToList()
            };
        }
    }
}
