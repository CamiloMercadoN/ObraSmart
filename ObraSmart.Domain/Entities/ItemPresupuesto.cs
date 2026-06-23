

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

        // Propiedades de Navegación
        public virtual Presupuesto? Presupuesto { get; set; }
        public virtual EstructuraAPU? EstructuraAPUOrigen { get; set; }
        public virtual ICollection<RecursoItemPresupuesto> Recursos { get; set; } = new List<RecursoItemPresupuesto>();
    }
}
