
namespace ObraSmart.Application.DTOs.Insumos
{
    public class ResumenProcesamientoDto
    {
        public int Procesados { get; set; }
        public int Actualizados { get; set; }
        public List<string> DetalleErrores { get; set; } = [];
    }
}
