

namespace ObraSmart.Domain.Entities
{
    public class ItemPresupuesto
    {
        public Guid Id { get; set; }
        public Guid PresupuestoId { get; set; }
        public Guid? EstructuraAPUOrigenId { get; set; } // Opcional: permite saber de qué plantilla se generó
        public string Descripcion { get; set; } = string.Empty;
        public decimal CantidadItem { get; set; }
        public decimal PrecioUnitarioCalculado { get; set; }
        public decimal Subtotal { get; set; }
        public int UnidadMedidaId { get; set; }
        

        // Propiedades de Navegación
        public Presupuesto? Presupuesto { get; set; }
        public EstructuraAPU? EstructuraAPUOrigen { get; set; }
        public ICollection<RecursoItemPresupuesto> Recursos { get; set; } = new List<RecursoItemPresupuesto>();
        public UnidadMedida? UnidadMedida { get; set; }
    }
}
