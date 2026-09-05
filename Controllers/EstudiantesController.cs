using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InscripcionesUniversidad.Data;
using InscripcionesUniversidad.Models;

namespace InscripcionesUniversidad.Controllers;

public class EstudiantesController : Controller
{
    private readonly JsonDataStore _store;

    public EstudiantesController(JsonDataStore store)
    {
        _store = store;
    }

    public IActionResult Index()
    {
        var estudiantes = _store.Estudiantes
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToList();
        return View(estudiantes);
    }

    public IActionResult Details(int? id)
    {
        if (id is null) return NotFound();

        var estudiante = _store.Estudiantes.FirstOrDefault(e => e.Id == id);
        if (estudiante is null) return NotFound();

        return View(estudiante);
    }

    public IActionResult Create()
    {
        CargarCarreras();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Nombres,Apellidos,Carne,Email,CarreraId")] Estudiante estudiante)
    {
        if (_store.Estudiantes.Any(e => e.Carne == estudiante.Carne))
        {
            ModelState.AddModelError(nameof(Estudiante.Carne), "Ya existe un estudiante con ese carné");
        }

        if (!ModelState.IsValid)
        {
            CargarCarreras(estudiante.CarreraId);
            return View(estudiante);
        }

        estudiante.Id = JsonDataStore.SiguienteId(_store.Estudiantes.Select(e => e.Id));
        _store.Estudiantes.Add(estudiante);
        _store.GuardarCambios();

        TempData["Mensaje"] = $"Estudiante \"{estudiante.NombreCompleto}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int? id)
    {
        if (id is null) return NotFound();

        var estudiante = _store.Estudiantes.FirstOrDefault(e => e.Id == id);
        if (estudiante is null) return NotFound();

        CargarCarreras(estudiante.CarreraId);
        return View(estudiante);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,Nombres,Apellidos,Carne,Email,CarreraId")] Estudiante estudiante)
    {
        if (id != estudiante.Id) return NotFound();

        if (_store.Estudiantes.Any(e => e.Carne == estudiante.Carne && e.Id != estudiante.Id))
        {
            ModelState.AddModelError(nameof(Estudiante.Carne), "Ya existe un estudiante con ese carné");
        }

        if (!ModelState.IsValid)
        {
            CargarCarreras(estudiante.CarreraId);
            return View(estudiante);
        }

        var existente = _store.Estudiantes.FirstOrDefault(e => e.Id == id);
        if (existente is null) return NotFound();

        existente.Nombres = estudiante.Nombres;
        existente.Apellidos = estudiante.Apellidos;
        existente.Carne = estudiante.Carne;
        existente.Email = estudiante.Email;
        existente.CarreraId = estudiante.CarreraId;
        _store.GuardarCambios();

        TempData["Mensaje"] = $"Estudiante \"{estudiante.NombreCompleto}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int? id)
    {
        if (id is null) return NotFound();

        var estudiante = _store.Estudiantes.FirstOrDefault(e => e.Id == id);
        if (estudiante is null) return NotFound();

        return View(estudiante);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var estudiante = _store.Estudiantes.FirstOrDefault(e => e.Id == id);
        if (estudiante is not null)
        {
            var tieneInscripciones = _store.Inscripciones.Any(i => i.EstudianteId == id);
            if (tieneInscripciones)
            {
                TempData["Mensaje"] = "No se puede eliminar el estudiante porque tiene inscripciones asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _store.Estudiantes.Remove(estudiante);
            _store.GuardarCambios();
            TempData["Mensaje"] = "Estudiante eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void CargarCarreras(int? carreraSeleccionada = null)
    {
        var carreras = _store.Carreras.OrderBy(c => c.Nombre).ToList();
        ViewData["CarreraId"] = new SelectList(carreras, nameof(Carrera.Id), nameof(Carrera.Nombre), carreraSeleccionada);
    }
}
