

namespace ObraSmart.Domain.Entities
{
    public class ComponenteAPU
    {
        public Guid Id { get; set; }
        public Guid EstructuraAPUId { get; set; }
        public Guid InsumoId { get; set; }
        public decimal Cantidad { get; set; }

        // Propiedades de Navegación
        public virtual EstructuraAPU? EstructuraAPU { get; set; }
        public virtual Insumo? Insumo { get; set; }
    }
}
