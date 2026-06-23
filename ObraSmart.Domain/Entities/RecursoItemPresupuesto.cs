

namespace ObraSmart.Domain.Entities
{
    public class RecursoItemPresupuesto
    {
        public Guid Id { get; set; }
        public Guid ItemPresupuestoId { get; set; }
        public string TipoInsumo { get; set; } = string.Empty;
        public string DescripcionCongelada { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitarioCongelado { get; set; }
        public decimal CostoTotalRecurso { get; set; }

        // Propiedades de Navegación
        public virtual ItemPresupuesto? ItemPresupuesto { get; set; }
    }
}
