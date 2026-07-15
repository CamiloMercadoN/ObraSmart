using ObraSmart.Domain.Interfaces;

namespace ObraSmart.Domain.Entities
{
    public class Insumo : IUserOwnedEntity
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string TipoInsumo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioReferencia { get; set; }
        public int UnidadMedidaId { get; set; }
        public bool EsPlantilla { get; set; }

        // Propiedades de Navegación
        public Usuario? Usuario { get; set; }
        public UnidadMedida? UnidadMedida { get; set; }
        public ICollection<Etiqueta> Etiquetas { get; set; } = new List<Etiqueta>();
    }
}
