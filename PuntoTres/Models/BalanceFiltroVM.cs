using System;
using System.ComponentModel.DataAnnotations;

namespace PuntoTres.Models
{
    public class BalanceFiltroVM
    {
        [Display(Name = "Desde"), DataType(DataType.Date)]
        public DateTime? Desde { get; set; }

        [Display(Name = "Hasta"), DataType(DataType.Date)]
        public DateTime? Hasta { get; set; }

        [Required, Display(Name = "Código interno")]
        public string CodigoInterno { get; set; }

        [Display(Name = "Total usado (g/ml)")]
        public decimal TotalUsado { get; set; }

        [Display(Name = "Registros")]
        public int Registros { get; set; }
    }
}
