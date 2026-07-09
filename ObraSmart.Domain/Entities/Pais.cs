using System;
using System.Collections.Generic;
using System.Text;

namespace ObraSmart.Domain.Entities
{
    public class Pais
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string CodigoIso { get; set; } = string.Empty; // Ej: "CL", "MX", "PE"

        public virtual ICollection<EstadoProvincia> EstadosProvincias { get; set; } = new List<EstadoProvincia>();
    }
}
