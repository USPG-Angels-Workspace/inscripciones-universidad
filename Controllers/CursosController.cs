using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InscripcionesUniversidad.Data;
using InscripcionesUniversidad.Models;

namespace InscripcionesUniversidad.Controllers;

public class CursosController : Controller
{
    private readonly JsonDataStore _store;

    public CursosController(JsonDataStore store)
    {
        _store = store;
    }

    public IActionResult Index()
    {
        var cursos = _store.Cursos.OrderBy(c => c.Nombre).ToList();
        return View(cursos);
    }

    public IActionResult Details(int? id)
    {
        if (id is null) return NotFound();

        var curso = _store.Cursos.FirstOrDefault(c => c.Id == id);
        if (curso is null) return NotFound();

        return View(curso);
    }

    public IActionResult Create()
    {
        CargarCarreras();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Nombre,Codigo,Creditos,CupoMaximo,CarreraId")] Curso curso)
    {
        if (_store.Cursos.Any(c => c.Codigo == curso.Codigo))
        {
            ModelState.AddModelError(nameof(Curso.Codigo), "Ya existe un curso con ese código");
        }

        if (!ModelState.IsValid)
        {
            CargarCarreras(curso.CarreraId);
            return View(curso);
        }

        curso.Id = JsonDataStore.SiguienteId(_store.Cursos.Select(c => c.Id));
        _store.Cursos.Add(curso);
        _store.GuardarCambios();

        TempData["Mensaje"] = $"Curso \"{curso.Nombre}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int? id)
    {
        if (id is null) return NotFound();

        var curso = _store.Cursos.FirstOrDefault(c => c.Id == id);
        if (curso is null) return NotFound();

        CargarCarreras(curso.CarreraId);
        return View(curso);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,Nombre,Codigo,Creditos,CupoMaximo,CarreraId")] Curso curso)
    {
        if (id != curso.Id) return NotFound();

        if (_store.Cursos.Any(c => c.Codigo == curso.Codigo && c.Id != curso.Id))
        {
            ModelState.AddModelError(nameof(Curso.Codigo), "Ya existe un curso con ese código");
        }

        if (!ModelState.IsValid)
        {
            CargarCarreras(curso.CarreraId);
            return View(curso);
        }

        var existente = _store.Cursos.FirstOrDefault(c => c.Id == id);
        if (existente is null) return NotFound();

        existente.Nombre = curso.Nombre;
        existente.Codigo = curso.Codigo;
        existente.Creditos = curso.Creditos;
        existente.CupoMaximo = curso.CupoMaximo;
        existente.CarreraId = curso.CarreraId;
        _store.GuardarCambios();

        TempData["Mensaje"] = $"Curso \"{curso.Nombre}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int? id)
    {
        if (id is null) return NotFound();

        var curso = _store.Cursos.FirstOrDefault(c => c.Id == id);
        if (curso is null) return NotFound();

        return View(curso);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var curso = _store.Cursos.FirstOrDefault(c => c.Id == id);
        if (curso is not null)
        {
            var tieneInscripciones = _store.Inscripciones.Any(i => i.CursoId == id);
            if (tieneInscripciones)
            {
                TempData["Mensaje"] = "No se puede eliminar el curso porque tiene inscripciones asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _store.Cursos.Remove(curso);
            _store.GuardarCambios();
            TempData["Mensaje"] = "Curso eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void CargarCarreras(int? carreraSeleccionada = null)
    {
        var carreras = _store.Carreras.OrderBy(c => c.Nombre).ToList();
        ViewData["CarreraId"] = new SelectList(carreras, nameof(Carrera.Id), nameof(Carrera.Nombre), carreraSeleccionada);
    }
}
