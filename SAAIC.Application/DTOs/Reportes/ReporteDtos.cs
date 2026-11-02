using SAAIC.Domain.Enums;

namespace SAAIC.Application.DTOs.Reportes;

public record FiltroReporteEventosRequestDto(
    DateOnly FechaInicio,
    DateOnly FechaFin,
    int? IdCampus = null,
    int? IdEspacio = null,
    string? EnviarPorCorreoA = null);

public record FiltroReporteAsistenciaRequestDto(
    DateOnly FechaInicio,
    DateOnly FechaFin,
    int? IdCampus = null,
    int? IdEspacio = null,
    string? IdUsuario = null,
    EstadoAsistencia? EstadoAsistencia = null,
    string? EnviarPorCorreoA = null);

public record ArchivoReporteDto(
    byte[] ContenidoBytes,
    string NombreArchivo,
    string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
