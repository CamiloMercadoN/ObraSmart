

using ObraSmart.Domain.Interfaces;

namespace ObraSmart.Domain.Entities
{
    public class EstructuraAPU : IUserOwnedEntity
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int UnidadMedidaId { get; set; }
        public decimal CostoTotalCalculado { get; set; }
        public bool EsPlantilla { get; set; }

        // Propiedades de Navegación
        public Usuario? Usuario { get; set; }
        public UnidadMedida? UnidadMedida { get; set; }
        public ICollection<ComponenteAPU> Componentes { get; set; } = new List<ComponenteAPU>();
        public ICollection<Etiqueta> Etiquetas { get; set; } = new List<Etiqueta>();
    }
}
