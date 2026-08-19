
namespace ObraSmart.Application.DTOs.Dashboard
{
    public class DashboardDto
    {
        public int PresupuestosActivos { get; set; }
        public int CotizacionesEnviadas { get; set; }
        public int ApusCreadas { get; set; }
        public int Pendientes { get; set; }
        public List<ProyectoRecienteDto> ProyectosRecientes { get; set; } = new();
    }
}
