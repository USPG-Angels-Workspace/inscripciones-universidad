using Microsoft.AspNetCore.Mvc;
using InscripcionesUniversidad.Data;
using InscripcionesUniversidad.Models;

namespace InscripcionesUniversidad.Controllers;

public class PeriodosController : Controller
{
    private readonly JsonDataStore _store;

    public PeriodosController(JsonDataStore store)
    {
        _store = store;
    }

    public IActionResult Index()
    {
        var periodos = _store.Periodos.OrderByDescending(p => p.FechaInicio).ToList();
        return View(periodos);
    }

    public IActionResult Details(int? id)
    {
        if (id is null) return NotFound();

        var periodo = _store.Periodos.FirstOrDefault(p => p.Id == id);
        if (periodo is null) return NotFound();

        return View(periodo);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Nombre,FechaInicio,FechaFin,Activo")] Periodo periodo)
    {
        if (periodo.FechaFin < periodo.FechaInicio)
        {
            ModelState.AddModelError(nameof(Periodo.FechaFin), "La fecha de fin debe ser posterior a la fecha de inicio");
        }

        if (!ModelState.IsValid) return View(periodo);

        periodo.Id = JsonDataStore.SiguienteId(_store.Periodos.Select(p => p.Id));
        _store.Periodos.Add(periodo);
        _store.GuardarCambios();

        TempData["Mensaje"] = $"Período \"{periodo.Nombre}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int? id)
    {
        if (id is null) return NotFound();

        var periodo = _store.Periodos.FirstOrDefault(p => p.Id == id);
        if (periodo is null) return NotFound();

        return View(periodo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,Nombre,FechaInicio,FechaFin,Activo")] Periodo periodo)
    {
        if (id != periodo.Id) return NotFound();

        if (periodo.FechaFin < periodo.FechaInicio)
        {
            ModelState.AddModelError(nameof(Periodo.FechaFin), "La fecha de fin debe ser posterior a la fecha de inicio");
        }

        if (!ModelState.IsValid) return View(periodo);

        var existente = _store.Periodos.FirstOrDefault(p => p.Id == id);
        if (existente is null) return NotFound();

        existente.Nombre = periodo.Nombre;
        existente.FechaInicio = periodo.FechaInicio;
        existente.FechaFin = periodo.FechaFin;
        existente.Activo = periodo.Activo;
        _store.GuardarCambios();

        TempData["Mensaje"] = $"Período \"{periodo.Nombre}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int? id)
    {
        if (id is null) return NotFound();

        var periodo = _store.Periodos.FirstOrDefault(p => p.Id == id);
        if (periodo is null) return NotFound();

        return View(periodo);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var periodo = _store.Periodos.FirstOrDefault(p => p.Id == id);
        if (periodo is not null)
        {
            var tieneInscripciones = _store.Inscripciones.Any(i => i.PeriodoId == id);
            if (tieneInscripciones)
            {
                TempData["Mensaje"] = "No se puede eliminar el período porque tiene inscripciones asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _store.Periodos.Remove(periodo);
            _store.GuardarCambios();
            TempData["Mensaje"] = "Período eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}
