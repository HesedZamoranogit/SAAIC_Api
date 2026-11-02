using SAAIC.Domain.Enums;

namespace SAAIC.Application.DTOs.Asistencias;

public record AsistenciaDto(
    int Id,
    int IdReserva,
    string IdUsuario,
    string NombreUsuario,
    DateOnly FechaEvento,
    TimeOnly HoraProgramadaInicio,
    TimeOnly HoraProgramadaFin,
    DateTime? HoraEntrada,
    DateTime? HoraSalida,
    EstadoAsistencia Estado,
    string EstadoDescripcion,
    int MinutosRetardo,
    int MinutosToleranciaAplicados,
    bool MarcoEntrada,
    bool MarcoSalida,
    string? Observaciones);

public record JustificarAsistenciaRequestDto(
    string MotivoJustificacion);

public record ConfiguracionSistemaDto(
    int Id,
    int MinutosToleranciaRetardo,
    int MinutosAnticipacionCheckIn,
    bool EnvioAutomaticoReportesActivo,
    TimeOnly HoraEnvioReporteEventos,
    TimeOnly HoraEnvioReporteAsistencia,
    string CorreosDestinatariosReportes);

public record ActualizarConfiguracionAsistenciaRequestDto(
    int MinutosToleranciaRetardo,
    int MinutosAnticipacionCheckIn);

public record ActualizarConfiguracionReportesRequestDto(
    bool EnvioAutomaticoReportesActivo,
    TimeOnly HoraEnvioReporteEventos,
    TimeOnly HoraEnvioReporteAsistencia,
    string? CorreosDestinatariosReportes);
