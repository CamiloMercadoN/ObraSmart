

namespace ObraSmart.Domain.Entities
{
    public class Cliente
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Rut { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        // Propiedades de Navegación
        public virtual Usuario? Usuario { get; set; }
        public virtual ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
    }
}
