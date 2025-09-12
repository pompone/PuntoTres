using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // 👈 necesario para [Precision]

namespace PuntoTres.Models
{
    public class SolucionPreparada
    {
        public int Id { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Fecha de preparación")]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Required, StringLength(30)]
        [Display(Name = "Código interno")]
        public string CodigoInterno { get; set; } = string.Empty;

        [Required, StringLength(60)]
        [Display(Name = "Marca")]
        public string Marca { get; set; } = string.Empty;

        [Required, StringLength(120)]
        [Display(Name = "Nombre de la solución")]
        public string Nombre { get; set; } = string.Empty;

        [Precision(18, 2)]
        [Display(Name = "Peso/Volumen usado")]
        [Range(0.0, 1_000_000)]
        public decimal? CantidadBase { get; set; }

        [Precision(18, 2)]
        [Required, Range(0.01, 1_000_000)]
        [Display(Name = "Volumen final")]
        public decimal VolumenFinal { get; set; }

        [StringLength(30)]
        [Display(Name = "Lote")]
        public string? Lote { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Concentración obtenida")]
        public string ConcentracionObtenida { get; set; } = string.Empty;

        [Required, StringLength(60)]
        [Display(Name = "ID del reactivo")]
        public string IdReactivo { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [NotMapped]
        public bool Vencida => FechaVencimiento.HasValue && FechaVencimiento.Value.Date < DateTime.Today;
    }
}
