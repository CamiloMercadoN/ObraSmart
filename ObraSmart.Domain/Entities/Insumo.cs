

namespace ObraSmart.Domain.Entities
{
    public class Insumo
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string TipoInsumo { get; set; } = string.Empty; // Ej: Material, Mano de Obra, Equipo
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioReferencia { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;

        // Propiedades de Navegación
        public virtual Usuario? Usuario { get; set; }
    }
}
