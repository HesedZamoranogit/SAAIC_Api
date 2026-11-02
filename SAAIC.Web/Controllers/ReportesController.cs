using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Reportes;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Domain.Common;

namespace SAAIC.Web.Controllers;

[Authorize(Roles = RolesSistema.Administrador)]
public class ReportesController : BaseApiController
{
    private readonly IReporteService _reporteService;

    public ReportesController(IReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    /// <summary>
    /// Genera y descarga al momento el Reporte 1 en Excel (Eventos programados y Estado de Espacios) aplicando filtros personalizados.
    /// No afecta el corte automático diario.
    /// </summary>
    [HttpPost("eventos/exportar")]
    public async Task<IActionResult> ExportarReporteEventos(
        [FromBody] FiltroReporteEventosRequestDto filtro,
        CancellationToken cancellationToken)
    {
        var archivo = await _reporteService.GenerarReporteEventosManualAsync(filtro, UsuarioActualId, cancellationToken);
        return File(archivo.ContenidoBytes, archivo.ContentType, archivo.NombreArchivo);
    }

    /// <summary>
    /// Genera y descarga al momento el Reporte 2 en Excel (Control de Asistencia, Retardos e Inasistencias) aplicando filtros personalizados.
    /// No afecta el corte automático diario.
    /// </summary>
    [HttpPost("asistencia/exportar")]
    public async Task<IActionResult> ExportarReporteAsistencia(
        [FromBody] FiltroReporteAsistenciaRequestDto filtro,
        CancellationToken cancellationToken)
    {
        var archivo = await _reporteService.GenerarReporteAsistenciaManualAsync(filtro, UsuarioActualId, cancellationToken);
        return File(archivo.ContenidoBytes, archivo.ContentType, archivo.NombreArchivo);
    }

    /// <summary>
    /// Permite a un Administrador disparar la ejecución de los reportes automáticos diarios para una fecha específica.
    /// </summary>
    [HttpPost("automaticos/ejecutar")]
    public async Task<IActionResult> EjecutarReportesAutomaticosDelDia(
        [FromQuery] DateOnly? fecha = null,
        CancellationToken cancellationToken = default)
    {
        var fechaObjetivo = fecha ?? DateOnly.FromDateTime(DateTime.Now);

        var enviadoEventos = await _reporteService.EjecutarReporteAutomaticoEventosDelDiaAsync(fechaObjetivo, cancellationToken);
        var enviadoAsistencia = await _reporteService.EjecutarReporteAutomaticoAsistenciaDelDiaAsync(fechaObjetivo, cancellationToken);

        return Ok(ApiResponse<object>.SuccessResult(
            new
            {
                Fecha = fechaObjetivo,
                ReporteEventosEnviado = enviadoEventos,
                ReporteAsistenciaEnviado = enviadoAsistencia
            },
            "Proceso de reportes automáticos diarios verificado y ejecutado."));
    }
}
