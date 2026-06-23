

namespace ObraSmart.Domain.Entities
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string Rut { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public decimal PorcentajeIva { get; set; }
        public int ValidezCotizacionDias { get; set; }
        public string FormaPagoPredeterminada { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;

        // Propiedades de Navegación
        public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
        public virtual ICollection<Insumo> Insumos { get; set; } = new List<Insumo>();
        public virtual ICollection<EstructuraAPU> EstructurasAPU { get; set; } = new List<EstructuraAPU>();
        public virtual ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
    }
}
