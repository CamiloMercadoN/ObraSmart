
namespace ObraSmart.Application.DTOs.APUs
{
    public class EstructuraAPUUpsertDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int UnidadMedidaId { get; set; }
        public List<Guid> EtiquetasIds { get; set; } = [];
        public List<ComponenteAPUInputDto> Componentes { get; set; } = [];
    }
}
