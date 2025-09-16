using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // Importamos esto para tener mas presicion

namespace PuntoTres.Models
{
    public class SolucionPreparada
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de preparación es obligatoria")]
        [DataType(DataType.Date, ErrorMessage = "Por favor, introduzca una fecha válida")]
        [Display(Name = "Fecha de preparación")]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "El código interno es obligatorio")]
        [StringLength(30, ErrorMessage = "El código interno no puede superar los 30 caracteres")]
        [Display(Name = "Código interno")]
        public string CodigoInterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es obligatoria")]
        [StringLength(60, ErrorMessage = "La marca no puede superar los 60 caracteres")]
        [Display(Name = "Marca")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la solución es obligatorio")]
        [StringLength(120, ErrorMessage = "El nombre no puede superar los 120 caracteres")]
        [Display(Name = "Nombre de la solución")]
        public string Nombre { get; set; } = string.Empty;

        [Precision(18, 2)]
        [Display(Name = "Peso/Volumen usado")]
        [Range(0.0, 1_000_000, ErrorMessage = "El valor debe estar entre 0 y 1.000.000")]
        public decimal? CantidadBase { get; set; }

        [Precision(18, 2)]
        [Required(ErrorMessage = "El volumen final es obligatorio")]
        [Range(0.01, 1_000_000, ErrorMessage = "El volumen final debe estar entre 0.01 y 1.000.000")]
        [Display(Name = "Volumen final")]
        public decimal VolumenFinal { get; set; }

        [StringLength(30, ErrorMessage = "El lote no puede superar los 30 caracteres")]
        [Display(Name = "Lote")]
        public string? Lote { get; set; }

        [Required(ErrorMessage = "La concentración obtenida es obligatoria")]
        [StringLength(50, ErrorMessage = "La concentración obtenida no puede superar los 50 caracteres")]
        [Display(Name = "Concentración obtenida")]
        public string ConcentracionObtenida { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID del reactivo es obligatorio")]
        [StringLength(60, ErrorMessage = "El ID del reactivo no puede superar los 60 caracteres")]
        [Display(Name = "ID del reactivo")]
        public string IdReactivo { get; set; } = string.Empty;

        [DataType(DataType.Date, ErrorMessage = "Por favor, introduzca una fecha válida")]
        [Display(Name = "Fecha de vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [NotMapped]
        public bool Vencida => FechaVencimiento.HasValue && FechaVencimiento.Value.Date < DateTime.Today;
    }
}


