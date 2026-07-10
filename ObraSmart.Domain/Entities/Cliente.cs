

using ObraSmart.Domain.Interfaces;

namespace ObraSmart.Domain.Entities
{
    public class Cliente : IUserOwnedEntity
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Rut { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int? CiudadId { get; set; }
        public bool EsPlantilla { get; set; }

        // Propiedades de Navegación
        public Usuario? Usuario { get; set; }
        public Ciudad? Ciudad { get; set; }
        public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
    }
}
