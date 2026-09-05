using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InscripcionesUniversidad.Data;
using InscripcionesUniversidad.Models;

namespace InscripcionesUniversidad.Controllers;

public class CarrerasController : Controller
{
    private readonly ApplicationDbContext _context;

    public CarrerasController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var carreras = await _context.Carreras
            .OrderBy(c => c.Nombre)
            .ToListAsync();
        return View(carreras);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var carrera = await _context.Carreras
            .Include(c => c.Cursos)
            .Include(c => c.Estudiantes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (carrera is null) return NotFound();

        return View(carrera);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Codigo")] Carrera carrera)
    {
        if (await _context.Carreras.AnyAsync(c => c.Codigo == carrera.Codigo))
        {
            ModelState.AddModelError(nameof(Carrera.Codigo), "Ya existe una carrera con ese código");
        }

        if (!ModelState.IsValid) return View(carrera);

        _context.Add(carrera);
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = $"Carrera \"{carrera.Nombre}\" creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var carrera = await _context.Carreras.FindAsync(id);
        if (carrera is null) return NotFound();

        return View(carrera);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Codigo")] Carrera carrera)
    {
        if (id != carrera.Id) return NotFound();

        if (await _context.Carreras.AnyAsync(c => c.Codigo == carrera.Codigo && c.Id != carrera.Id))
        {
            ModelState.AddModelError(nameof(Carrera.Codigo), "Ya existe una carrera con ese código");
        }

        if (!ModelState.IsValid) return View(carrera);

        try
        {
            _context.Update(carrera);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Carreras.AnyAsync(c => c.Id == id)) return NotFound();
            throw;
        }

        TempData["Mensaje"] = $"Carrera \"{carrera.Nombre}\" actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var carrera = await _context.Carreras.FirstOrDefaultAsync(c => c.Id == id);
        if (carrera is null) return NotFound();

        return View(carrera);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var carrera = await _context.Carreras.FindAsync(id);
        if (carrera is not null)
        {
            var tieneCursos = await _context.Cursos.AnyAsync(c => c.CarreraId == id);
            var tieneEstudiantes = await _context.Estudiantes.AnyAsync(e => e.CarreraId == id);

            if (tieneCursos || tieneEstudiantes)
            {
                TempData["Mensaje"] = "No se puede eliminar la carrera porque tiene cursos o estudiantes asociados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Carreras.Remove(carrera);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Carrera eliminada correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}
