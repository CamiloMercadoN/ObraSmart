

namespace ObraSmart.Application.DTOs.Cotizaciones
{
    public class CrearCotizacionRequestDto
    {
        public Guid PresupuestoId { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int? NumeroCotizacionPersonalizado { get; set; }
    }
}
