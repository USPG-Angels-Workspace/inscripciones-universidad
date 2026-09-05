using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InscripcionesUniversidad.Data;
using InscripcionesUniversidad.Models;

namespace InscripcionesUniversidad.Controllers;

public class PeriodosController : Controller
{
    private readonly ApplicationDbContext _context;

    public PeriodosController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var periodos = await _context.Periodos
            .OrderByDescending(p => p.FechaInicio)
            .ToListAsync();
        return View(periodos);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var periodo = await _context.Periodos.FirstOrDefaultAsync(p => p.Id == id);
        if (periodo is null) return NotFound();

        return View(periodo);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,FechaInicio,FechaFin,Activo")] Periodo periodo)
    {
        if (periodo.FechaFin < periodo.FechaInicio)
        {
            ModelState.AddModelError(nameof(Periodo.FechaFin), "La fecha de fin debe ser posterior a la fecha de inicio");
        }

        if (!ModelState.IsValid) return View(periodo);

        _context.Add(periodo);
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = $"Período \"{periodo.Nombre}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var periodo = await _context.Periodos.FindAsync(id);
        if (periodo is null) return NotFound();

        return View(periodo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,FechaInicio,FechaFin,Activo")] Periodo periodo)
    {
        if (id != periodo.Id) return NotFound();

        if (periodo.FechaFin < periodo.FechaInicio)
        {
            ModelState.AddModelError(nameof(Periodo.FechaFin), "La fecha de fin debe ser posterior a la fecha de inicio");
        }

        if (!ModelState.IsValid) return View(periodo);

        try
        {
            _context.Update(periodo);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Periodos.AnyAsync(p => p.Id == id)) return NotFound();
            throw;
        }

        TempData["Mensaje"] = $"Período \"{periodo.Nombre}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var periodo = await _context.Periodos.FirstOrDefaultAsync(p => p.Id == id);
        if (periodo is null) return NotFound();

        return View(periodo);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var periodo = await _context.Periodos.FindAsync(id);
        if (periodo is not null)
        {
            var tieneInscripciones = await _context.Inscripciones.AnyAsync(i => i.PeriodoId == id);
            if (tieneInscripciones)
            {
                TempData["Mensaje"] = "No se puede eliminar el período porque tiene inscripciones asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Periodos.Remove(periodo);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Período eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}
