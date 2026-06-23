

namespace ObraSmart.Domain.Entities
{
    public class EstructuraAPU
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal CostoTotalCalculado { get; set; }

        // Propiedades de Navegación
        public virtual Usuario? Usuario { get; set; }
        public virtual ICollection<ComponenteAPU> Componentes { get; set; } = new List<ComponenteAPU>();
    }
}
