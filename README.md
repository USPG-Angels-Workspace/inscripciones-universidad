# Inscripciones Universidad

Sistema web para controlar las inscripciones de una universidad: administra
carreras, cursos, períodos académicos, estudiantes e inscripciones, validando
cupo máximo por curso y evitando inscripciones duplicadas.

## Tecnologías

- ASP.NET Core MVC (.NET 10)
- Persistencia en archivo JSON (`App_Data/database.json`)
- Tailwind CSS (vía CDN)

## Ejecutar el proyecto

```bash
dotnet restore
dotnet run
```

Los datos se guardan en `App_Data/database.json`, que se crea
automáticamente al registrar el primer dato si aún no existe.

## Documentación

Ver [docs/entidades.md](docs/entidades.md) para el detalle de las entidades
del dominio.
