using System;
using System.Collections.Generic;
using System.Text;

namespace ObraSmart.Domain.Entities
{
    public class Ciudad
    {
        public int Id { get; set; }
        public int EstadoProvinciaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string CodigoLocal { get; set; } = string.Empty;

        public EstadoProvincia EstadoProvincia { get; set; } = null!;
        public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
