
namespace ObraSmart.Application.DTOs.APUs
{
    public class ComponenteAPUDto
    {
        public Guid InsumoId { get; set; }
        public string DescripcionInsumo { get; set; } = string.Empty;
        public string TipoInsumo { get; set; } = string.Empty;
        public decimal PrecioUnitarioReferencia { get; set; } // Viene del Insumo actual
        public decimal Cantidad { get; set; }
        public decimal Subtotal => Math.Round(PrecioUnitarioReferencia * Cantidad, 2);
    }
}
