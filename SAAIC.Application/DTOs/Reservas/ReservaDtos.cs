using SAAIC.Application.DTOs.Asistencias;
using SAAIC.Domain.Enums;

namespace SAAIC.Application.DTOs.Reservas;

public record CrearReservaPorPeriodoRequestDto(
    int IdEspacio,
    string ResponsableEvento,
    string Descripcion,
    int MaximoAsistentes,
    DateOnly Fecha,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string? NotaSeguridad = null);

/// <summary>
/// Petición para reservar por jornada completa (de 1 a 7 días dentro de una ventana máxima de 7 días).
/// Se puede enviar una lista de fechas específicas (FechasSeleccionadas) o un rango continuo (FechaInicio a FechaFin).
/// </summary>
public record CrearReservaPorJornadaRequestDto(
    int IdEspacio,
    string ResponsableEvento,
    string Descripcion,
    int MaximoAsistentes,
    IReadOnlyList<DateOnly>? FechasSeleccionadas = null,
    DateOnly? FechaInicio = null,
    DateOnly? FechaFin = null,
    string? NotaSeguridad = null);

public record CrearReservaSemestralRequestDto(
    int IdEspacio,
    string ResponsableEvento,
    string Descripcion,
    int MaximoAsistentes,
    DateOnly FechaInicioSemestre,
    DateOnly FechaFinSemestre,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    DiasSemana DiasSeleccionados,
    string? NotaSeguridad = null);

public record CancelarReservaRequestDto(
    string? MotivoCancelacion = null);

public record ReasignarReservaRequestDto(
    string NuevoIdUsuario,
    string NuevoResponsableEvento);

public record ReservaInstanciaDiariaDto(
    int Id,
    int? IdReservaPadre,
    int IdEspacio,
    string NombreEspacio,
    string NombreTipoEspacio,
    int IdCampus,
    string NombreCampus,
    string IdUsuario,
    string NombreUsuario,
    string ResponsableEvento,
    string Descripcion,
    int MaximoAsistentes,
    string? NotaSeguridad,
    TipoReserva TipoReserva,
    EstadoReserva Estado,
    DateOnly Fecha,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    AsistenciaDto? Asistencia);

public record ReservaDto(
    int Id,
    int? IdReservaPadre,
    bool EsReservaPadre,
    int IdEspacio,
    string NombreEspacio,
    string NombreTipoEspacio,
    int IdCampus,
    string NombreCampus,
    string IdUsuario,
    string NombreUsuario,
    string ResponsableEvento,
    string Descripcion,
    int MaximoAsistentes,
    string? NotaSeguridad,
    TipoReserva TipoReserva,
    string TipoReservaDescripcion,
    EstadoReserva Estado,
    string EstadoDescripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    DiasSemana DiasRecurrencia,
    DateTime? FechaCancelacion,
    string? MotivoCancelacion,
    AsistenciaDto? Asistencia,
    IReadOnlyList<ReservaInstanciaDiariaDto> InstanciasHijas);
