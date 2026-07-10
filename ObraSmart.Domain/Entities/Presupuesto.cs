

using ObraSmart.Domain.Interfaces;

namespace ObraSmart.Domain.Entities
{
    public class Presupuesto : IUserOwnedEntity
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid? ClienteId { get; set; }
        public string NombreProyecto { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; } = string.Empty; // Borrador, Emitido, Aprobado, Rechazado
        public decimal Subtotal { get; set; }
        public decimal MontoIva { get; set; }
        public decimal Total { get; set; }
        public bool EsPlantilla { get; set; }

        // Propiedades de Navegación
        public Usuario? Usuario { get; set; }
        public Cliente? Cliente { get; set; }
        public ICollection<ItemPresupuesto> Items { get; set; } = new List<ItemPresupuesto>();
        public Cotizacion? Cotizacion { get; set; } // Relación 1:1 o 1:0
    }
}
