
namespace ObraSmart.Application.DTOs.Cotizaciones
{
    public class CotizacionResponseDto
    {
        public Guid Id { get; set; }
        public Guid PresupuestoId { get; set; }
        public string NumeroCotizacion { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string ArchivoPdfUrl { get; set; } = string.Empty;
        public string NombreProyecto { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
    }
}
