
namespace ObraSmart.Application.DTOs.APUs
{
    public class EstructuraAPUDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int UnidadMedidaId { get; set; }
        public string UnidadMedidaNombre { get; set; } = string.Empty;
        public decimal CostoTotalCalculado { get; set; }
        public bool EsPlantilla { get; set; }

        public List<Guid> EtiquetasIds { get; set; } = [];
        public List<ComponenteAPUDto> Componentes { get; set; } = [];
    }
}
