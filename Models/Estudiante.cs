using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InscripcionesUniversidad.Models;

public class Estudiante
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(60)]
    [Display(Name = "Nombres")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(60)]
    [Display(Name = "Apellidos")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El carné es obligatorio")]
    [StringLength(20)]
    [Display(Name = "Carné")]
    public string Carne { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo inválido")]
    [StringLength(120)]
    [Display(Name = "Correo")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Carrera")]
    public int CarreraId { get; set; }

    [JsonIgnore]
    public Carrera? Carrera { get; set; }

    [JsonIgnore]
    public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();

    [JsonIgnore]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto => $"{Nombres} {Apellidos}";
}
