

namespace ObraSmart.Application.DTOs.Clientes
{
    public class ClienteResponseDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Rut { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int? CiudadId { get; set; }
        public string CiudadNombre { get; set; } = string.Empty;
        public string EstadoProvinciaNombre { get; set; } = string.Empty;
        public int? RegionId { get; set; }
    }
}
