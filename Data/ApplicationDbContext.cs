using Microsoft.EntityFrameworkCore;
using InscripcionesUniversidad.Models;

namespace InscripcionesUniversidad.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Carrera> Carreras => Set<Carrera>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Periodo> Periodos => Set<Periodo>();
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
    public DbSet<Inscripcion> Inscripciones => Set<Inscripcion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Carrera>()
            .HasIndex(c => c.Codigo)
            .IsUnique();

        modelBuilder.Entity<Curso>()
            .HasIndex(c => c.Codigo)
            .IsUnique();

        modelBuilder.Entity<Estudiante>()
            .HasIndex(e => e.Carne)
            .IsUnique();

        modelBuilder.Entity<Curso>()
            .HasOne(c => c.Carrera)
            .WithMany(car => car.Cursos)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Estudiante>()
            .HasOne(e => e.Carrera)
            .WithMany(car => car.Estudiantes)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Inscripcion>()
            .HasOne(i => i.Estudiante)
            .WithMany(e => e.Inscripciones)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Inscripcion>()
            .HasOne(i => i.Curso)
            .WithMany(c => c.Inscripciones)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Inscripcion>()
            .HasOne(i => i.Periodo)
            .WithMany(p => p.Inscripciones)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
