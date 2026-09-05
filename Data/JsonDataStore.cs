using System.Text.Json;
using InscripcionesUniversidad.Models;

namespace InscripcionesUniversidad.Data;

public class JsonDataStore
{
    private static readonly object _lock = new();
    private readonly string _filePath;
    private readonly DataModel _data;

    public JsonDataStore(IWebHostEnvironment env)
    {
        _filePath = Path.Combine(env.ContentRootPath, "App_Data", "database.json");
        _data = Cargar();
        RelinkNavegaciones();
    }

    public List<Carrera> Carreras => _data.Carreras;
    public List<Curso> Cursos => _data.Cursos;
    public List<Periodo> Periodos => _data.Periodos;
    public List<Estudiante> Estudiantes => _data.Estudiantes;
    public List<Inscripcion> Inscripciones => _data.Inscripciones;

    public static int SiguienteId(IEnumerable<int> idsExistentes) =>
        idsExistentes.Any() ? idsExistentes.Max() + 1 : 1;

    public void GuardarCambios()
    {
        lock (_lock)
        {
            RelinkNavegaciones();
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }

    private DataModel Cargar()
    {
        lock (_lock)
        {
            if (!File.Exists(_filePath)) return new DataModel();

            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json)) return new DataModel();

            return JsonSerializer.Deserialize<DataModel>(json) ?? new DataModel();
        }
    }

    private void RelinkNavegaciones()
    {
        foreach (var curso in _data.Cursos)
            curso.Carrera = _data.Carreras.FirstOrDefault(c => c.Id == curso.CarreraId);

        foreach (var estudiante in _data.Estudiantes)
            estudiante.Carrera = _data.Carreras.FirstOrDefault(c => c.Id == estudiante.CarreraId);

        foreach (var inscripcion in _data.Inscripciones)
        {
            inscripcion.Estudiante = _data.Estudiantes.FirstOrDefault(e => e.Id == inscripcion.EstudianteId);
            inscripcion.Curso = _data.Cursos.FirstOrDefault(c => c.Id == inscripcion.CursoId);
            inscripcion.Periodo = _data.Periodos.FirstOrDefault(p => p.Id == inscripcion.PeriodoId);
        }

        foreach (var carrera in _data.Carreras)
        {
            carrera.Cursos = _data.Cursos.Where(c => c.CarreraId == carrera.Id).ToList();
            carrera.Estudiantes = _data.Estudiantes.Where(e => e.CarreraId == carrera.Id).ToList();
        }

        foreach (var curso in _data.Cursos)
            curso.Inscripciones = _data.Inscripciones.Where(i => i.CursoId == curso.Id).ToList();

        foreach (var estudiante in _data.Estudiantes)
            estudiante.Inscripciones = _data.Inscripciones.Where(i => i.EstudianteId == estudiante.Id).ToList();

        foreach (var periodo in _data.Periodos)
            periodo.Inscripciones = _data.Inscripciones.Where(i => i.PeriodoId == periodo.Id).ToList();
    }
}

internal class DataModel
{
    public List<Carrera> Carreras { get; set; } = new();
    public List<Curso> Cursos { get; set; } = new();
    public List<Periodo> Periodos { get; set; } = new();
    public List<Estudiante> Estudiantes { get; set; } = new();
    public List<Inscripcion> Inscripciones { get; set; } = new();
}
