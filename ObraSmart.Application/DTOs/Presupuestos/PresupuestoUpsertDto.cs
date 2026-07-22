

namespace ObraSmart.Application.DTOs.Presupuestos
{
    public class PresupuestoUpsertDto
    {
        public Guid? ClienteId { get; set; }
        public string NombreProyecto { get; set; } = string.Empty;
        public List<ItemPresupuestoUpsertDto> Items { get; set; } = new List<ItemPresupuestoUpsertDto>();
    }
}
