

namespace ObraSmart.Domain.Entities
{
    public class Cotizacion
    {
        public Guid Id { get; set; }
        public Guid PresupuestoId { get; set; }
        public string NumeroCotizacion { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string ArchivoPdfUrl { get; set; } = string.Empty;

        // Propiedades de Navegación
        public virtual Presupuesto? Presupuesto { get; set; }
    }
}
