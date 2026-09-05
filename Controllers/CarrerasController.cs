using Microsoft.AspNetCore.Mvc;
using InscripcionesUniversidad.Data;
using InscripcionesUniversidad.Models;

namespace InscripcionesUniversidad.Controllers;

public class CarrerasController : Controller
{
    private readonly JsonDataStore _store;

    public CarrerasController(JsonDataStore store)
    {
        _store = store;
    }

    public IActionResult Index()
    {
        var carreras = _store.Carreras.OrderBy(c => c.Nombre).ToList();
        return View(carreras);
    }

    public IActionResult Details(int? id)
    {
        if (id is null) return NotFound();

        var carrera = _store.Carreras.FirstOrDefault(c => c.Id == id);
        if (carrera is null) return NotFound();

        return View(carrera);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Nombre,Codigo")] Carrera carrera)
    {
        if (_store.Carreras.Any(c => c.Codigo == carrera.Codigo))
        {
            ModelState.AddModelError(nameof(Carrera.Codigo), "Ya existe una carrera con ese código");
        }

        if (!ModelState.IsValid) return View(carrera);

        carrera.Id = JsonDataStore.SiguienteId(_store.Carreras.Select(c => c.Id));
        _store.Carreras.Add(carrera);
        _store.GuardarCambios();

        TempData["Mensaje"] = $"Carrera \"{carrera.Nombre}\" creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int? id)
    {
        if (id is null) return NotFound();

        var carrera = _store.Carreras.FirstOrDefault(c => c.Id == id);
        if (carrera is null) return NotFound();

        return View(carrera);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,Nombre,Codigo")] Carrera carrera)
    {
        if (id != carrera.Id) return NotFound();

        if (_store.Carreras.Any(c => c.Codigo == carrera.Codigo && c.Id != carrera.Id))
        {
            ModelState.AddModelError(nameof(Carrera.Codigo), "Ya existe una carrera con ese código");
        }

        if (!ModelState.IsValid) return View(carrera);

        var existente = _store.Carreras.FirstOrDefault(c => c.Id == id);
        if (existente is null) return NotFound();

        existente.Nombre = carrera.Nombre;
        existente.Codigo = carrera.Codigo;
        _store.GuardarCambios();

        TempData["Mensaje"] = $"Carrera \"{carrera.Nombre}\" actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int? id)
    {
        if (id is null) return NotFound();

        var carrera = _store.Carreras.FirstOrDefault(c => c.Id == id);
        if (carrera is null) return NotFound();

        return View(carrera);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var carrera = _store.Carreras.FirstOrDefault(c => c.Id == id);
        if (carrera is not null)
        {
            var tieneCursos = _store.Cursos.Any(c => c.CarreraId == id);
            var tieneEstudiantes = _store.Estudiantes.Any(e => e.CarreraId == id);

            if (tieneCursos || tieneEstudiantes)
            {
                TempData["Mensaje"] = "No se puede eliminar la carrera porque tiene cursos o estudiantes asociados.";
                return RedirectToAction(nameof(Index));
            }

            _store.Carreras.Remove(carrera);
            _store.GuardarCambios();
            TempData["Mensaje"] = "Carrera eliminada correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}
