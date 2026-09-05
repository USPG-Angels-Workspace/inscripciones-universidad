using System.ComponentModel.DataAnnotations;

namespace InscripcionesUniversidad.Models;

public class Periodo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateTime FechaInicio { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de fin")]
    public DateTime FechaFin { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
}
