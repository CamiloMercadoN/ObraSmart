
namespace ObraSmart.Application.DTOs.Etiquetas
{
    public class EtiquetaDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ColorHex { get; set; } = string.Empty;
        public bool EsPlantilla { get; set; }
    }
}
