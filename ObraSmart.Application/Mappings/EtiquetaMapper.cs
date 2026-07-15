using ObraSmart.Application.DTOs.Etiquetas;
using ObraSmart.Domain.Entities;

namespace ObraSmart.Application.Mappings
{
    public static class EtiquetaMapper
    {
        public static EtiquetaDto ToDto(this Etiqueta entity)
        {
            if (entity == null) return null!;
            return new EtiquetaDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                ColorHex = entity.ColorHex,
                EsPlantilla = entity.EsPlantilla
            };
        }

        public static Etiqueta ToEntity(this EtiquetaUpsertDto dto)
        {
            if (dto == null) return null!;
            return new Etiqueta
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                ColorHex = dto.ColorHex,
                EsPlantilla = false
            };
        }

        public static void UpdateEntity(this EtiquetaUpsertDto dto, Etiqueta entity)
        {
            if (dto == null || entity == null) return;
            entity.Nombre = dto.Nombre;
            entity.ColorHex = dto.ColorHex;
        }
    }
}
