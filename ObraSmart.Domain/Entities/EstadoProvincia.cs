using System;
using System.Collections.Generic;
using System.Text;

namespace ObraSmart.Domain.Entities
{
    public class EstadoProvincia
    {
        public int Id { get; set; }
        public int PaisId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string CodigoLocal { get; set; } = string.Empty; // Ej: "RM"

        public virtual Pais Pais { get; set; } = null!;
        public virtual ICollection<Ciudad> Ciudades { get; set; } = new List<Ciudad>();
    }
}
