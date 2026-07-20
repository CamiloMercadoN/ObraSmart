using ObraSmart.Application.DTOs.APUs;
using ObraSmart.Domain.Entities;

namespace ObraSmart.Application.Mappings
{
    public static class EstructuraAPUMapper
    {
        public static EstructuraAPUDto ToDto(this EstructuraAPU entity)
        {
            return new EstructuraAPUDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                UnidadMedidaId = entity.UnidadMedidaId,
                UnidadMedidaNombre = entity.UnidadMedida?.Nombre ?? string.Empty,
                CostoTotalCalculado = entity.CostoTotalCalculado,
                EsPlantilla = entity.EsPlantilla,
                EtiquetasIds = entity.Etiquetas.Select(e => e.Id).ToList(),
                Componentes = entity.Componentes.Select(c => new ComponenteAPUDto
                {
                    InsumoId = c.InsumoId,
                    DescripcionInsumo = c.Insumo?.Descripcion ?? string.Empty,
                    TipoInsumo = c.Insumo?.TipoInsumo ?? string.Empty,
                    PrecioUnitarioReferencia = c.Insumo?.PrecioReferencia ?? 0,
                    Cantidad = c.Cantidad
                }).ToList()
            };
        }
    }
}
