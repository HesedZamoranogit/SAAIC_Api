using SAAIC.Application.Common.Exceptions;
using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Asistencias;
using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Application.Mappings;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;
using SAAIC.Domain.Exceptions;

namespace SAAIC.Application.Services;

public class AsistenciaService : IAsistenciaService
{
    private readonly IAsistenciaRepository _asistenciaRepository;
    private readonly IConfiguracionSistemaRepository _configuracionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AsistenciaService(
        IAsistenciaRepository asistenciaRepository,
        IConfiguracionSistemaRepository configuracionRepository,
        IUnitOfWork unitOfWork)
    {
        _asistenciaRepository = asistenciaRepository;
        _configuracionRepository = configuracionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<AsistenciaDto>>> ObtenerAsistenciasPaginadasAsync(
        DateOnly? fechaInicio = null,
        DateOnly? fechaFin = null,
        int? idCampus = null,
        int? idEspacio = null,
        string? idUsuario = null,
        EstadoAsistencia? estado = null,
        PagedRequest? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = pagination?.PageNumber ?? 1;
        var pageSize = pagination?.PageSize ?? 10;

        var asistenciasPaginadas = await _asistenciaRepository.ObtenerPaginadoConFiltrosAsync(
            fechaInicio, fechaFin, idCampus, idEspacio, idUsuario, estado, pageNumber, pageSize, cancellationToken);

        var pagedDtos = asistenciasPaginadas.MapTo(a => a.ToDto());
        return ApiResponse<PagedResult<AsistenciaDto>>.SuccessResult(pagedDtos);
    }

    public async Task<ApiResponse<AsistenciaDto>> RegistrarEntradaAsync(
        int idReservaInstancia,
        string idUsuarioActual,
        CancellationToken cancellationToken = default)
    {
        var asistencia = await _asistenciaRepository.ObtenerPorIdReservaAsync(idReservaInstancia, cancellationToken)
            ?? throw new NotFoundException(nameof(Asistencia), idReservaInstancia);

        if (asistencia.IdUsuario != idUsuarioActual)
        {
            throw new DomainException("Solo el profesor responsable de la reservación puede registrar su entrada.");
        }

        if (asistencia.Reserva?.Estado == EstadoReserva.Cancelada)
        {
            throw new DomainException("No se puede registrar entrada en una reservación cancelada.");
        }

        var config = await _configuracionRepository.ObtenerConfiguracionActivaAsync(cancellationToken);

        asistencia.RegistrarEntrada(
            DateTime.Now,
            config.MinutosToleranciaRetardo,
            config.MinutosAnticipacionCheckIn);

        await _asistenciaRepository.UpdateAsync(asistencia, cancellationToken);

        var mensaje = asistencia.Estado == EstadoAsistencia.Retardo
            ? $"Entrada registrada con retardo ({asistencia.MinutosRetardo} min)."
            : "Entrada registrada a tiempo.";

        return ApiResponse<AsistenciaDto>.SuccessResult(asistencia.ToDto(), mensaje);
    }

    public async Task<ApiResponse<AsistenciaDto>> RegistrarSalidaAsync(
        int idReservaInstancia,
        string idUsuarioActual,
        CancellationToken cancellationToken = default)
    {
        var asistencia = await _asistenciaRepository.ObtenerPorIdReservaAsync(idReservaInstancia, cancellationToken)
            ?? throw new NotFoundException(nameof(Asistencia), idReservaInstancia);

        if (asistencia.IdUsuario != idUsuarioActual)
        {
            throw new DomainException("Solo el profesor responsable de la reservación puede registrar su salida.");
        }

        asistencia.RegistrarSalida(DateTime.Now);

        await _asistenciaRepository.UpdateAsync(asistencia, cancellationToken);
        return ApiResponse<AsistenciaDto>.SuccessResult(asistencia.ToDto(), "Salida registrada exitosamente.");
    }

    public async Task<ApiResponse<AsistenciaDto>> JustificarAsistenciaAsync(
        int idReservaInstancia,
        JustificarAsistenciaRequestDto request,
        string adminUsuarioId,
        CancellationToken cancellationToken = default)
    {
        var asistencia = await _asistenciaRepository.ObtenerPorIdReservaAsync(idReservaInstancia, cancellationToken)
            ?? throw new NotFoundException(nameof(Asistencia), idReservaInstancia);

        asistencia.Justificar(adminUsuarioId, request.MotivoJustificacion);

        await _asistenciaRepository.UpdateAsync(asistencia, cancellationToken);
        return ApiResponse<AsistenciaDto>.SuccessResult(asistencia.ToDto(), "Asistencia justificada exitosamente.");
    }

    public async Task<int> ProcesarInasistenciasVencidasAsync(
        DateOnly fecha,
        TimeOnly horaCorteActual,
        CancellationToken cancellationToken = default)
    {
        var pendientesVencidas = await _asistenciaRepository.ObtenerPendientesVencidasAsync(
            fecha,
            horaCorteActual,
            cancellationToken);

        if (pendientesVencidas.Count == 0)
            return 0;

        foreach (var asistencia in pendientesVencidas)
        {
            asistencia.MarcarInasistencia();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return pendientesVencidas.Count;
    }

    public async Task<ApiResponse<ConfiguracionSistemaDto>> ObtenerConfiguracionSistemaAsync(
        CancellationToken cancellationToken = default)
    {
        var config = await _configuracionRepository.ObtenerConfiguracionActivaAsync(cancellationToken);
        return ApiResponse<ConfiguracionSistemaDto>.SuccessResult(config.ToDto());
    }

    public async Task<ApiResponse<ConfiguracionSistemaDto>> ActualizarConfiguracionAsistenciaAsync(
        ActualizarConfiguracionAsistenciaRequestDto request,
        string adminUsuarioId,
        CancellationToken cancellationToken = default)
    {
        var config = await _configuracionRepository.ObtenerConfiguracionActivaAsync(cancellationToken);

        config.ActualizarConfiguracionAsistencia(
            request.MinutosToleranciaRetardo,
            request.MinutosAnticipacionCheckIn,
            adminUsuarioId);

        await _configuracionRepository.UpdateAsync(config, cancellationToken);

        return ApiResponse<ConfiguracionSistemaDto>.SuccessResult(
            config.ToDto(),
            $"Tiempo de tolerancia para retardo actualizado a {config.MinutosToleranciaRetardo} minutos.");
    }

    public async Task<ApiResponse<ConfiguracionSistemaDto>> ActualizarConfiguracionReportesAsync(
        ActualizarConfiguracionReportesRequestDto request,
        string adminUsuarioId,
        CancellationToken cancellationToken = default)
    {
        var config = await _configuracionRepository.ObtenerConfiguracionActivaAsync(cancellationToken);

        config.ActualizarConfiguracionReportes(
            request.EnvioAutomaticoReportesActivo,
            request.HoraEnvioReporteEventos,
            request.HoraEnvioReporteAsistencia,
            request.CorreosDestinatariosReportes,
            adminUsuarioId);

        await _configuracionRepository.UpdateAsync(config, cancellationToken);

        return ApiResponse<ConfiguracionSistemaDto>.SuccessResult(
            config.ToDto(),
            "Configuración de reportes diarios actualizada exitosamente.");
    }
}
