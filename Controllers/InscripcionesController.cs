using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InscripcionesUniversidad.Data;
using InscripcionesUniversidad.Models;

namespace InscripcionesUniversidad.Controllers;

public class InscripcionesController : Controller
{
    private readonly ApplicationDbContext _context;

    public InscripcionesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var inscripciones = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Include(i => i.Curso)
            .Include(i => i.Periodo)
            .OrderByDescending(i => i.FechaInscripcion)
            .ToListAsync();
        return View(inscripciones);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var inscripcion = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Include(i => i.Curso)
            .Include(i => i.Periodo)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inscripcion is null) return NotFound();

        return View(inscripcion);
    }

    public async Task<IActionResult> Create()
    {
        await CargarListas();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EstudianteId,CursoId,PeriodoId")] Inscripcion inscripcion)
    {
        await ValidarReglasDeNegocio(inscripcion);

        if (!ModelState.IsValid)
        {
            await CargarListas(inscripcion.EstudianteId, inscripcion.CursoId, inscripcion.PeriodoId);
            return View(inscripcion);
        }

        inscripcion.FechaInscripcion = DateTime.Now;
        inscripcion.Estado = EstadoInscripcion.Activa;

        _context.Add(inscripcion);
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "Inscripción registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var inscripcion = await _context.Inscripciones.FindAsync(id);
        if (inscripcion is null) return NotFound();

        await CargarListas(inscripcion.EstudianteId, inscripcion.CursoId, inscripcion.PeriodoId);
        return View(inscripcion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,EstudianteId,CursoId,PeriodoId,FechaInscripcion,Estado")] Inscripcion inscripcion)
    {
        if (id != inscripcion.Id) return NotFound();

        await ValidarReglasDeNegocio(inscripcion, ignorarId: inscripcion.Id);

        if (!ModelState.IsValid)
        {
            await CargarListas(inscripcion.EstudianteId, inscripcion.CursoId, inscripcion.PeriodoId);
            return View(inscripcion);
        }

        try
        {
            _context.Update(inscripcion);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Inscripciones.AnyAsync(i => i.Id == id)) return NotFound();
            throw;
        }

        TempData["Mensaje"] = "Inscripción actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var inscripcion = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Include(i => i.Curso)
            .Include(i => i.Periodo)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inscripcion is null) return NotFound();

        return View(inscripcion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var inscripcion = await _context.Inscripciones.FindAsync(id);
        if (inscripcion is not null)
        {
            _context.Inscripciones.Remove(inscripcion);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Inscripción eliminada correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarReglasDeNegocio(Inscripcion inscripcion, int? ignorarId = null)
    {
        var yaInscrito = await _context.Inscripciones.AnyAsync(i =>
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

        var curso = await _context.Cursos.FindAsync(inscripcion.CursoId);
        if (curso is null) return;

        var inscritosActivos = await _context.Inscripciones.CountAsync(i =>
            i.Id != ignorarId &&
            i.CursoId == inscripcion.CursoId &&
            i.PeriodoId == inscripcion.PeriodoId &&
            i.Estado == EstadoInscripcion.Activa);

        if (inscripcion.Estado != EstadoInscripcion.Anulada && inscritosActivos >= curso.CupoMaximo)
        {
            ModelState.AddModelError(string.Empty, $"No hay cupo disponible en \"{curso.Nombre}\" para el período seleccionado ({inscritosActivos}/{curso.CupoMaximo}).");
        }
    }

    private async Task CargarListas(int? estudianteSeleccionado = null, int? cursoSeleccionado = null, int? periodoSeleccionado = null)
    {
        var estudiantes = await _context.Estudiantes.OrderBy(e => e.Apellidos).ThenBy(e => e.Nombres).ToListAsync();
        var cursos = await _context.Cursos.OrderBy(c => c.Nombre).ToListAsync();
        var periodos = await _context.Periodos.OrderByDescending(p => p.FechaInicio).ToListAsync();

        ViewData["EstudianteId"] = new SelectList(
            estudiantes.Select(e => new { e.Id, NombreCompleto = $"{e.Carne} - {e.NombreCompleto}" }),
            "Id", "NombreCompleto", estudianteSeleccionado);
        ViewData["CursoId"] = new SelectList(cursos, nameof(Curso.Id), nameof(Curso.Nombre), cursoSeleccionado);
        ViewData["PeriodoId"] = new SelectList(periodos, nameof(Periodo.Id), nameof(Periodo.Nombre), periodoSeleccionado);
    }
}
