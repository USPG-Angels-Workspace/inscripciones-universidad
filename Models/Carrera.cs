using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InscripcionesUniversidad.Models;

public class Carrera
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código es obligatorio")]
    [StringLength(10)]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Curso> Cursos { get; set; } = new List<Curso>();

    [JsonIgnore]
    public ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();
}
