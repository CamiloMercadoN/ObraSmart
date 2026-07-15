using ObraSmart.Application.DTOs.UnidadesMedida;
using ObraSmart.Domain.Entities;

namespace ObraSmart.Application.Mappings
{
    public static class UnidadMedidaMapper
    {
        public static UnidadMedidaDto ToDto(this UnidadMedida entity)
        {
            if (entity == null) return null!;
            return new UnidadMedidaDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Abreviacion = entity.Abreviacion
            };
        }
    }
}
