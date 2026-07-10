

namespace ObraSmart.Domain.Entities
{
    public class ComponenteAPU
    {
        public Guid Id { get; set; }
        public Guid EstructuraAPUId { get; set; }
        public Guid InsumoId { get; set; }
        public decimal Cantidad { get; set; }

        // Propiedades de Navegación
        public EstructuraAPU? EstructuraAPU { get; set; }
        public Insumo? Insumo { get; set; }
    }
}
