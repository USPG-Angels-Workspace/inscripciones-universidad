# Inscripciones Universidad

Sistema web para controlar las inscripciones de una universidad: administra
carreras, cursos, períodos académicos, estudiantes e inscripciones, validando
cupo máximo por curso y evitando inscripciones duplicadas.

## Tecnologías

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core con SQLite
- Tailwind CSS (vía CDN)

## Ejecutar el proyecto

```bash
dotnet restore
dotnet run
```

Las migraciones de la base de datos se aplican automáticamente al iniciar
la aplicación.

## Documentación

Ver [docs/entidades.md](docs/entidades.md) para el detalle de las entidades
del dominio.
