using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InscripcionesUniversidad.Models;

public class Curso
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

    [Range(1, 10, ErrorMessage = "Los créditos deben estar entre 1 y 10")]
    [Display(Name = "Créditos")]
    public int Creditos { get; set; }

    [Range(1, 500, ErrorMessage = "El cupo máximo debe ser mayor a 0")]
    [Display(Name = "Cupo máximo")]
    public int CupoMaximo { get; set; }

    [Required]
    [Display(Name = "Carrera")]
    public int CarreraId { get; set; }

    [JsonIgnore]
    public Carrera? Carrera { get; set; }

    [JsonIgnore]
    public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
}
