# Sistema de Inscripciones de la Universidad

## Entidades

### Carrera
- Id
- Nombre
- Codigo

### Curso
- Id
- Nombre
- Codigo
- Creditos
- CupoMaximo
- CarreraId (FK a Carrera)

### Periodo
- Id
- Nombre
- FechaInicio
- FechaFin
- Activo

### Estudiante
- Id
- Nombres
- Apellidos
- Carne
- Email
- CarreraId (FK a Carrera)

### Inscripcion
- Id
- EstudianteId (FK a Estudiante)
- CursoId (FK a Curso)
- PeriodoId (FK a Periodo)
- FechaInscripcion
- Estado (Activa / Anulada)
