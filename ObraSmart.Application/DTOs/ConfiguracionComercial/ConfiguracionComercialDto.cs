using System;
using System.Collections.Generic;
using System.Text;

namespace ObraSmart.Application.DTOs.ConfiguracionComercial
{
    public class ConfiguracionComercialDto
    {
        public string RazonSocial { get; set; } = string.Empty;
        public decimal PorcentajeIva { get; set; }
        public int DiasValidez { get; set; }

        // En consultas devolverá la URL estática ("/uploads/..."). 
        // Al guardar, si el usuario subió imagen, traerá el string "data:image/png;base64,..."
        public string? LogoBase64 { get; set; }
    }
}
