using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InscripcionesUniversidad.Models;

public class Inscripcion
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Estudiante")]
    public int EstudianteId { get; set; }

    [JsonIgnore]
    public Estudiante? Estudiante { get; set; }

    [Required]
    [Display(Name = "Curso")]
    public int CursoId { get; set; }

    [JsonIgnore]
    public Curso? Curso { get; set; }

    [Required]
    [Display(Name = "Período")]
    public int PeriodoId { get; set; }

    [JsonIgnore]
    public Periodo? Periodo { get; set; }

    [Display(Name = "Fecha de inscripción")]
    public DateTime FechaInscripcion { get; set; } = DateTime.Now;

    [Display(Name = "Estado")]
    public EstadoInscripcion Estado { get; set; } = EstadoInscripcion.Activa;
}
