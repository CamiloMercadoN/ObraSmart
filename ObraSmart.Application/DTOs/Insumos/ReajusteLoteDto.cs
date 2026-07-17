using System;
using System.Collections.Generic;
using System.Text;

namespace ObraSmart.Application.DTOs.Insumos
{
    public class ReajusteLoteDto
    {
        public string? TipoInsumo { get; set; }
        public Guid? EtiquetaId { get; set; } 
        public bool EsPorcentaje { get; set; } 
        public decimal Valor { get; set; }
    }
}
