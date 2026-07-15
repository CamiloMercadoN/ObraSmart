using ObraSmart.Application.DTOs.Insumos;
using ObraSmart.Domain.Entities;

namespace ObraSmart.Application.Mappings
{
    public static class InsumoMapper
    {
        // Mapeo de Entidad a Response DTO (Lectura)
        public static InsumoDto ToDto(this Insumo insumo)
        {
            if (insumo == null) return null!;

            return new InsumoDto
            {
                Id = insumo.Id,
                TipoInsumo = insumo.TipoInsumo,
                Descripcion = insumo.Descripcion,
                PrecioReferencia = insumo.PrecioReferencia,
                UnidadMedidaId = insumo.UnidadMedidaId,
                UnidadMedidaNombre = insumo.UnidadMedida?.Nombre ?? string.Empty,
                EsPlantilla = insumo.EsPlantilla,
                EtiquetasIds = insumo.Etiquetas?.Select(e => e.Id).ToList() ?? new List<Guid>()
            };
        }

        // Mapeo de Request DTO a Entidad (Creación)
        // Nota: La asignación física de objetos de relación (Etiquetas) se resuelve en el servicio
        public static Insumo ToEntity(this InsumoUpsertDto dto)
        {
            if (dto == null) return null!;

            return new Insumo
            {
                Id = Guid.NewGuid(),
                TipoInsumo = dto.TipoInsumo,
                Descripcion = dto.Descripcion,
                PrecioReferencia = dto.PrecioReferencia,
                UnidadMedidaId = dto.UnidadMedidaId,
                EsPlantilla = false,
                Etiquetas = new List<Etiqueta>() // Inicialización de colección vacía
            };
        }

        // Actualización de una entidad existente desde un DTO
        // Nota: Las relaciones muchos a muchos se limpian y reasignan de forma rastreada en el servicio
        public static void UpdateEntity(this InsumoUpsertDto dto, Insumo insumo)
        {
            if (dto == null || insumo == null) return;

            insumo.TipoInsumo = dto.TipoInsumo;
            insumo.Descripcion = dto.Descripcion;
            insumo.PrecioReferencia = dto.PrecioReferencia;
            insumo.UnidadMedidaId = dto.UnidadMedidaId;
        }
    }
}
