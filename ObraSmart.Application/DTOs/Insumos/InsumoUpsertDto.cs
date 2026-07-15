
namespace ObraSmart.Application.DTOs.Insumos
{
    public class InsumoUpsertDto
    {
        public string TipoInsumo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioReferencia { get; set; }
        public int UnidadMedidaId { get; set; }

        public List<Guid> EtiquetasIds { get; set; } = new();
    }
}
