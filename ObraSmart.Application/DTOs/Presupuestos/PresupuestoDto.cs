
namespace ObraSmart.Application.DTOs.Presupuestos
{
    public class PresupuestoDto
    {
        public Guid Id { get; set; }
        public Guid? ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string NombreProyecto { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal MontoIva { get; set; }
        public decimal Total { get; set; }
        public bool EsPlantilla { get; set; }
        public List<ItemPresupuestoDto> Items { get; set; } = new List<ItemPresupuestoDto>();
    }
}
