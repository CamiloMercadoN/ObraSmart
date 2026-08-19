
namespace ObraSmart.Application.DTOs.Dashboard
{
    public class ProyectoRecienteDto
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
