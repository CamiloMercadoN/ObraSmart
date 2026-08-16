using ObraSmart.Application.DTOs.Cotizaciones;
using ObraSmart.Domain.Entities;

namespace ObraSmart.Application.Mappings
{
    public static class CotizacionMapper
    {
        public static CotizacionResponseDto ToResponseDto(this Cotizacion cotizacion)
        {
            return new CotizacionResponseDto
            {
                Id = cotizacion.Id,
                PresupuestoId = cotizacion.PresupuestoId,
                NumeroCotizacion = cotizacion.NumeroCotizacion,
                FechaEmision = cotizacion.FechaEmision,
                FechaVencimiento = cotizacion.FechaVencimiento,
                Estado = cotizacion.Estado,
                ArchivoPdfUrl = cotizacion.ArchivoPdfUrl,
                NombreProyecto = cotizacion.Presupuesto?.NombreProyecto ?? "Sin Proyecto",
                ClienteNombre = cotizacion.Presupuesto?.Cliente?.Nombre ?? "Sin Cliente"
            };
        }
    }
}
