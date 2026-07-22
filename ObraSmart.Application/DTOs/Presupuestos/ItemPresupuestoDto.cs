
namespace ObraSmart.Application.DTOs.Presupuestos
{
    public class ItemPresupuestoDto
    {
        public Guid Id { get; set; }
        public Guid? EstructuraAPUOrigenId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal CantidadItem { get; set; }
        public decimal PrecioUnitarioCalculado { get; set; }
        public decimal Subtotal { get; set; }
        public int UnidadMedidaId { get; set; }
        public List<RecursoItemPresupuestoDto> Recursos { get; set; } = new List<RecursoItemPresupuestoDto>();
    }
}
