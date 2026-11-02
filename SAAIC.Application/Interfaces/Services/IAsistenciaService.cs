using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Asistencias;
using SAAIC.Domain.Enums;

namespace SAAIC.Application.Interfaces.Services;

public interface IAsistenciaService
{
    Task<ApiResponse<PagedResult<AsistenciaDto>>> ObtenerAsistenciasPaginadasAsync(
        DateOnly? fechaInicio = null,
        DateOnly? fechaFin = null,
        int? idCampus = null,
        int? idEspacio = null,
        string? idUsuario = null,
        EstadoAsistencia? estado = null,
        PagedRequest? pagination = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<AsistenciaDto>> RegistrarEntradaAsync(
        int idReservaInstancia,
        string idUsuarioActual,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<AsistenciaDto>> RegistrarSalidaAsync(
        int idReservaInstancia,
        string idUsuarioActual,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<AsistenciaDto>> JustificarAsistenciaAsync(
        int idReservaInstancia,
        JustificarAsistenciaRequestDto request,
        string adminUsuarioId,
        CancellationToken cancellationToken = default);

    Task<int> ProcesarInasistenciasVencidasAsync(
        DateOnly fecha,
        TimeOnly horaCorteActual,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ConfiguracionSistemaDto>> ObtenerConfiguracionSistemaAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ConfiguracionSistemaDto>> ActualizarConfiguracionAsistenciaAsync(
        ActualizarConfiguracionAsistenciaRequestDto request,
        string adminUsuarioId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ConfiguracionSistemaDto>> ActualizarConfiguracionReportesAsync(
        ActualizarConfiguracionReportesRequestDto request,
        string adminUsuarioId,
        CancellationToken cancellationToken = default);
}
