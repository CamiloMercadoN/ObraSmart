using ObraSmart.Domain.Interfaces;

namespace ObraSmart.Domain.Entities
{
    public class Etiqueta : IUserOwnedEntity
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public bool EsPlantilla { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#808080";

        // Relaciones Muchos a Muchos
        public ICollection<Insumo> Insumos { get; set; } = new List<Insumo>();
        public ICollection<EstructuraAPU> EstructurasAPU { get; set; } = new List<EstructuraAPU>();
    }
}
