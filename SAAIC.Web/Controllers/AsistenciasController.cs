using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Asistencias;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Domain.Common;
using SAAIC.Domain.Enums;

namespace SAAIC.Web.Controllers;

[Authorize]
public class AsistenciasController : BaseApiController
{
    private readonly IAsistenciaService _asistenciaService;

    public AsistenciasController(IAsistenciaService asistenciaService)
    {
        _asistenciaService = asistenciaService;
    }

    /// <summary>
    /// Consulta el historial de asistencias de forma paginada aplicando filtros opcionales.
    /// Si el usuario es Profesor, solo consulta sus propias asistencias; si es Administrador, puede consultar todas o filtrar por profesor.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerAsistencias(
        [FromQuery] DateOnly? fechaInicio = null,
        [FromQuery] DateOnly? fechaFin = null,
        [FromQuery] int? idCampus = null,
        [FromQuery] int? idEspacio = null,
        [FromQuery] string? idUsuario = null,
        [FromQuery] EstadoAsistencia? estado = null,
        [FromQuery] PagedRequest? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var usuarioFiltro = EsAdministrador ? idUsuario : UsuarioActualId;
        var result = await _asistenciaService.ObtenerAsistenciasPaginadasAsync(
            fechaInicio, fechaFin, idCampus, idEspacio, usuarioFiltro, estado, pagination, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Registra la entrada (Check-In) del profesor responsable en la instancia diaria del evento.
    /// Evalúa si es Presente o Retardo según el tiempo configurable en el sistema.
    /// </summary>
    [HttpPost("reserva/{idReservaInstancia:int}/entrada")]
    public async Task<IActionResult> RegistrarEntrada(int idReservaInstancia, CancellationToken cancellationToken)
    {
        var result = await _asistenciaService.RegistrarEntradaAsync(idReservaInstancia, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Registra la salida (Check-Out) del profesor responsable en la instancia diaria del evento.
    /// </summary>
    [HttpPost("reserva/{idReservaInstancia:int}/salida")]
    public async Task<IActionResult> RegistrarSalida(int idReservaInstancia, CancellationToken cancellationToken)
    {
        var result = await _asistenciaService.RegistrarSalidaAsync(idReservaInstancia, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Permite a un Administrador justificar una inasistencia o retardo en una instancia de evento.
    /// </summary>
    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpPost("reserva/{idReservaInstancia:int}/justificar")]
    public async Task<IActionResult> Justificar(
        int idReservaInstancia,
        [FromBody] JustificarAsistenciaRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _asistenciaService.JustificarAsistenciaAsync(idReservaInstancia, request, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene la configuración activa del sistema (minutos de tolerancia de retardo y programación de reportes).
    /// </summary>
    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpGet("configuracion")]
    public async Task<IActionResult> ObtenerConfiguracion(CancellationToken cancellationToken)
    {
        var result = await _asistenciaService.ObtenerConfiguracionSistemaAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Actualiza el tiempo de tolerancia para retardo y minutos de anticipación de Check-In.
    /// </summary>
    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpPut("configuracion/tolerancia")]
    public async Task<IActionResult> ActualizarTolerancia(
        [FromBody] ActualizarConfiguracionAsistenciaRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _asistenciaService.ActualizarConfiguracionAsistenciaAsync(request, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Actualiza los horarios de corte y destinatarios de correo de los reportes automáticos diarios.
    /// </summary>
    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpPut("configuracion/reportes")]
    public async Task<IActionResult> ActualizarConfiguracionReportes(
        [FromBody] ActualizarConfiguracionReportesRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _asistenciaService.ActualizarConfiguracionReportesAsync(request, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }
}
