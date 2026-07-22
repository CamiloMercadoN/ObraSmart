

namespace ObraSmart.Application.DTOs.Presupuestos
{
    public class ItemPresupuestoUpsertDto
    {
        public Guid? Id { get; set; }
        public Guid? EstructuraAPUOrigenId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal CantidadItem { get; set; }
        public int UnidadMedidaId { get; set; }
        public List<RecursoItemPresupuestoUpsertDto> Recursos { get; set; } = new List<RecursoItemPresupuestoUpsertDto>();
    }
}
