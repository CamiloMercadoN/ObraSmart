
namespace ObraSmart.Application.DTOs.Insumos
{
    public class InsumoDto
    {
        public Guid Id { get; set; }
        public string TipoInsumo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioReferencia { get; set; }
        public int UnidadMedidaId { get; set; }
        public string UnidadMedidaNombre { get; set; } = string.Empty;
        public bool EsPlantilla { get; set; }

        public List<Guid> EtiquetasIds { get; set; } = new();
    }
}
