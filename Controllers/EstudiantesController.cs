using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InscripcionesUniversidad.Data;
using InscripcionesUniversidad.Models;

namespace InscripcionesUniversidad.Controllers;

public class EstudiantesController : Controller
{
    private readonly ApplicationDbContext _context;

    public EstudiantesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var estudiantes = await _context.Estudiantes
            .Include(e => e.Carrera)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();
        return View(estudiantes);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var estudiante = await _context.Estudiantes
            .Include(e => e.Carrera)
            .Include(e => e.Inscripciones).ThenInclude(i => i.Curso)
            .Include(e => e.Inscripciones).ThenInclude(i => i.Periodo)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (estudiante is null) return NotFound();

        return View(estudiante);
    }

    public async Task<IActionResult> Create()
    {
        await CargarCarreras();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombres,Apellidos,Carne,Email,CarreraId")] Estudiante estudiante)
    {
        if (await _context.Estudiantes.AnyAsync(e => e.Carne == estudiante.Carne))
        {
            ModelState.AddModelError(nameof(Estudiante.Carne), "Ya existe un estudiante con ese carné");
        }

        if (!ModelState.IsValid)
        {
            await CargarCarreras(estudiante.CarreraId);
            return View(estudiante);
        }

        _context.Add(estudiante);
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = $"Estudiante \"{estudiante.NombreCompleto}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var estudiante = await _context.Estudiantes.FindAsync(id);
        if (estudiante is null) return NotFound();

        await CargarCarreras(estudiante.CarreraId);
        return View(estudiante);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombres,Apellidos,Carne,Email,CarreraId")] Estudiante estudiante)
    {
        if (id != estudiante.Id) return NotFound();

        if (await _context.Estudiantes.AnyAsync(e => e.Carne == estudiante.Carne && e.Id != estudiante.Id))
        {
            ModelState.AddModelError(nameof(Estudiante.Carne), "Ya existe un estudiante con ese carné");
        }

        if (!ModelState.IsValid)
        {
            await CargarCarreras(estudiante.CarreraId);
            return View(estudiante);
        }

        try
        {
            _context.Update(estudiante);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Estudiantes.AnyAsync(e => e.Id == id)) return NotFound();
            throw;
        }

        TempData["Mensaje"] = $"Estudiante \"{estudiante.NombreCompleto}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var estudiante = await _context.Estudiantes
            .Include(e => e.Carrera)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (estudiante is null) return NotFound();

        return View(estudiante);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var estudiante = await _context.Estudiantes.FindAsync(id);
        if (estudiante is not null)
        {
            var tieneInscripciones = await _context.Inscripciones.AnyAsync(i => i.EstudianteId == id);
            if (tieneInscripciones)
            {
                TempData["Mensaje"] = "No se puede eliminar el estudiante porque tiene inscripciones asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Estudiantes.Remove(estudiante);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Estudiante eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCarreras(int? carreraSeleccionada = null)
    {
        var carreras = await _context.Carreras.OrderBy(c => c.Nombre).ToListAsync();
        ViewData["CarreraId"] = new SelectList(carreras, nameof(Carrera.Id), nameof(Carrera.Nombre), carreraSeleccionada);
    }
}
