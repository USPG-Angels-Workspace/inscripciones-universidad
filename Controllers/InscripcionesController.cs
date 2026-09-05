using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InscripcionesUniversidad.Data;
using InscripcionesUniversidad.Models;

namespace InscripcionesUniversidad.Controllers;

public class InscripcionesController : Controller
{
    private readonly JsonDataStore _store;

    public InscripcionesController(JsonDataStore store)
    {
        _store = store;
    }

    public IActionResult Index()
    {
        var inscripciones = _store.Inscripciones
            .OrderByDescending(i => i.FechaInscripcion)
            .ToList();
        return View(inscripciones);
    }

    public IActionResult Details(int? id)
    {
        if (id is null) return NotFound();

        var inscripcion = _store.Inscripciones.FirstOrDefault(i => i.Id == id);
        if (inscripcion is null) return NotFound();

        return View(inscripcion);
    }

    public IActionResult Create()
    {
        CargarListas();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("EstudianteId,CursoId,PeriodoId")] Inscripcion inscripcion)
    {
        ValidarReglasDeNegocio(inscripcion);

        if (!ModelState.IsValid)
        {
            CargarListas(inscripcion.EstudianteId, inscripcion.CursoId, inscripcion.PeriodoId);
            return View(inscripcion);
        }

        inscripcion.Id = JsonDataStore.SiguienteId(_store.Inscripciones.Select(i => i.Id));
        inscripcion.FechaInscripcion = DateTime.Now;
        inscripcion.Estado = EstadoInscripcion.Activa;

        _store.Inscripciones.Add(inscripcion);
        _store.GuardarCambios();

        TempData["Mensaje"] = "Inscripción registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int? id)
    {
        if (id is null) return NotFound();

        var inscripcion = _store.Inscripciones.FirstOrDefault(i => i.Id == id);
        if (inscripcion is null) return NotFound();

        CargarListas(inscripcion.EstudianteId, inscripcion.CursoId, inscripcion.PeriodoId);
        return View(inscripcion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,EstudianteId,CursoId,PeriodoId,FechaInscripcion,Estado")] Inscripcion inscripcion)
    {
        if (id != inscripcion.Id) return NotFound();

        ValidarReglasDeNegocio(inscripcion, ignorarId: inscripcion.Id);

        if (!ModelState.IsValid)
        {
            CargarListas(inscripcion.EstudianteId, inscripcion.CursoId, inscripcion.PeriodoId);
            return View(inscripcion);
        }

        var existente = _store.Inscripciones.FirstOrDefault(i => i.Id == id);
        if (existente is null) return NotFound();

        existente.EstudianteId = inscripcion.EstudianteId;
        existente.CursoId = inscripcion.CursoId;
        existente.PeriodoId = inscripcion.PeriodoId;
        existente.Estado = inscripcion.Estado;
        _store.GuardarCambios();

        TempData["Mensaje"] = "Inscripción actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int? id)
    {
        if (id is null) return NotFound();

        var inscripcion = _store.Inscripciones.FirstOrDefault(i => i.Id == id);
        if (inscripcion is null) return NotFound();

        return View(inscripcion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var inscripcion = _store.Inscripciones.FirstOrDefault(i => i.Id == id);
        if (inscripcion is not null)
        {
            _store.Inscripciones.Remove(inscripcion);
            _store.GuardarCambios();
            TempData["Mensaje"] = "Inscripción eliminada correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void ValidarReglasDeNegocio(Inscripcion inscripcion, int? ignorarId = null)
    {
        var yaInscrito = _store.Inscripciones.Any(i =>
            i.Id != ignorarId &&
            i.EstudianteId == inscripcion.EstudianteId &&
            i.CursoId == inscripcion.CursoId &&
            i.PeriodoId == inscripcion.PeriodoId &&
            i.Estado == EstadoInscripcion.Activa);

        if (yaInscrito)
        {
            ModelState.AddModelError(string.Empty, "El estudiante ya está inscrito en este curso para el período seleccionado.");
            return;
        }

        var curso = _store.Cursos.FirstOrDefault(c => c.Id == inscripcion.CursoId);
        if (curso is null) return;

        var inscritosActivos = _store.Inscripciones.Count(i =>
            i.Id != ignorarId &&
            i.CursoId == inscripcion.CursoId &&
            i.PeriodoId == inscripcion.PeriodoId &&
            i.Estado == EstadoInscripcion.Activa);

        if (inscripcion.Estado != EstadoInscripcion.Anulada && inscritosActivos >= curso.CupoMaximo)
        {
            ModelState.AddModelError(string.Empty, $"No hay cupo disponible en \"{curso.Nombre}\" para el período seleccionado ({inscritosActivos}/{curso.CupoMaximo}).");
        }
    }

    private void CargarListas(int? estudianteSeleccionado = null, int? cursoSeleccionado = null, int? periodoSeleccionado = null)
    {
        var estudiantes = _store.Estudiantes.OrderBy(e => e.Apellidos).ThenBy(e => e.Nombres).ToList();
        var cursos = _store.Cursos.OrderBy(c => c.Nombre).ToList();
        var periodos = _store.Periodos.OrderByDescending(p => p.FechaInicio).ToList();

        ViewData["EstudianteId"] = new SelectList(
            estudiantes.Select(e => new { e.Id, NombreCompleto = $"{e.Carne} - {e.NombreCompleto}" }),
            "Id", "NombreCompleto", estudianteSeleccionado);
        ViewData["CursoId"] = new SelectList(cursos, nameof(Curso.Id), nameof(Curso.Nombre), cursoSeleccionado);
        ViewData["PeriodoId"] = new SelectList(periodos, nameof(Periodo.Id), nameof(Periodo.Nombre), periodoSeleccionado);
    }
}
