using ObraSmart.Application.DTOs.ConfiguracionComercial;
using ObraSmart.Domain.Entities;

namespace ObraSmart.Application.Mappings
{
    public static class ConfiguracionComercialMapper
    {
        public static ConfiguracionComercialDto ToDto(this Usuario usuario)
        {
            return new ConfiguracionComercialDto
            {
                RazonSocial = usuario.RazonSocial,
                PorcentajeIva = usuario.PorcentajeIva,
                DiasValidez = usuario.ValidezCotizacionDias,
                LogoBase64 = usuario.LogoUrl // Mapeamos la URL a la propiedad del front
            };
        }

        public static void UpdateEntity(this Usuario usuario, ConfiguracionComercialDto dto, string? nuevaLogoUrl)
        {
            usuario.RazonSocial = dto.RazonSocial;
            usuario.PorcentajeIva = dto.PorcentajeIva;
            usuario.ValidezCotizacionDias = dto.DiasValidez;

            // Solo actualizamos la URL si se procesó un archivo nuevo
            if (!string.IsNullOrWhiteSpace(nuevaLogoUrl))
            {
                usuario.LogoUrl = nuevaLogoUrl;
            }
        }
    }
}
