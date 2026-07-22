
namespace ObraSmart.Application.DTOs.Presupuestos
{
    public class RecursoItemPresupuestoDto
    {
        public Guid Id { get; set; }
        public string TipoInsumo { get; set; } = string.Empty;
        public string DescripcionCongelada { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitarioCongelado { get; set; }
        public decimal CostoTotalRecurso { get; set; }
        public int UnidadMedidaId { get; set; }
    }
}
