using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InscripcionesUniversidad.Data;
using InscripcionesUniversidad.Models;

namespace InscripcionesUniversidad.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;

    public CursosController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var cursos = await _context.Cursos
            .Include(c => c.Carrera)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
        return View(cursos);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var curso = await _context.Cursos
            .Include(c => c.Carrera)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (curso is null) return NotFound();

        return View(curso);
    }

    public async Task<IActionResult> Create()
    {
        await CargarCarreras();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Codigo,Creditos,CupoMaximo,CarreraId")] Curso curso)
    {
        if (await _context.Cursos.AnyAsync(c => c.Codigo == curso.Codigo))
        {
            ModelState.AddModelError(nameof(Curso.Codigo), "Ya existe un curso con ese código");
        }

        if (!ModelState.IsValid)
        {
            await CargarCarreras(curso.CarreraId);
            return View(curso);
        }

        _context.Add(curso);
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = $"Curso \"{curso.Nombre}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var curso = await _context.Cursos.FindAsync(id);
        if (curso is null) return NotFound();

        await CargarCarreras(curso.CarreraId);
        return View(curso);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Codigo,Creditos,CupoMaximo,CarreraId")] Curso curso)
    {
        if (id != curso.Id) return NotFound();

        if (await _context.Cursos.AnyAsync(c => c.Codigo == curso.Codigo && c.Id != curso.Id))
        {
            ModelState.AddModelError(nameof(Curso.Codigo), "Ya existe un curso con ese código");
        }

        if (!ModelState.IsValid)
        {
            await CargarCarreras(curso.CarreraId);
            return View(curso);
        }

        try
        {
            _context.Update(curso);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Cursos.AnyAsync(c => c.Id == id)) return NotFound();
            throw;
        }

        TempData["Mensaje"] = $"Curso \"{curso.Nombre}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var curso = await _context.Cursos
            .Include(c => c.Carrera)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (curso is null) return NotFound();

        return View(curso);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso is not null)
        {
            var tieneInscripciones = await _context.Inscripciones.AnyAsync(i => i.CursoId == id);
            if (tieneInscripciones)
            {
                TempData["Mensaje"] = "No se puede eliminar el curso porque tiene inscripciones asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Curso eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCarreras(int? carreraSeleccionada = null)
    {
        var carreras = await _context.Carreras.OrderBy(c => c.Nombre).ToListAsync();
        ViewData["CarreraId"] = new SelectList(carreras, nameof(Carrera.Id), nameof(Carrera.Nombre), carreraSeleccionada);
    }
}
