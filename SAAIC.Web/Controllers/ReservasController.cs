using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Reservas;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Domain.Common;
using SAAIC.Domain.Enums;

namespace SAAIC.Web.Controllers;

[Authorize]
public class ReservasController : BaseApiController
{
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    /// <summary>
    /// Obtiene el listado de reservaciones de forma paginada aplicando filtros opcionales.
    /// Si el usuario es Profesor, solo consulta sus propias reservas; si es Administrador, puede consultar todas o filtrar por profesor.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodas(
        [FromQuery] int? idCampus = null,
        [FromQuery] int? idEspacio = null,
        [FromQuery] string? idUsuario = null,
        [FromQuery] EstadoReserva? estado = null,
        [FromQuery] DateOnly? fechaInicio = null,
        [FromQuery] DateOnly? fechaFin = null,
        [FromQuery] PagedRequest? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var usuarioFiltro = EsAdministrador ? idUsuario : UsuarioActualId;
        var result = await _reservaService.ObtenerTodasAsync(
            idCampus, idEspacio, usuarioFiltro, estado, fechaInicio, fechaFin, pagination, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Consulta las instancias diarias de eventos dentro de un rango de fechas (para el calendario de disponibilidad).
    /// </summary>
    [HttpGet("calendario")]
    public async Task<IActionResult> ObtenerCalendario(
        [FromQuery] DateOnly fechaInicio,
        [FromQuery] DateOnly fechaFin,
        [FromQuery] int? idCampus = null,
        [FromQuery] int? idEspacio = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _reservaService.ObtenerCalendarioInstanciasAsync(
            fechaInicio, fechaFin, idCampus, idEspacio, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene las reservaciones del usuario autenticado de forma paginada (incluyendo instancias hijas y estado de asistencia).
    /// </summary>
    [HttpGet("mis-reservas")]
    public async Task<IActionResult> ObtenerMisReservas([FromQuery] PagedRequest? pagination, CancellationToken cancellationToken)
    {
        var result = await _reservaService.ObtenerMisReservasAsync(UsuarioActualId, pagination, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene el detalle completo de una reservación (Padre o instancia Hija).
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var result = await _reservaService.ObtenerPorIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Crea una reservación por periodo de horas dentro de un día (asignación inmediata First-Come, First-Served).
    /// </summary>
    [HttpPost("por-periodo")]
    public async Task<IActionResult> CrearPorPeriodo([FromBody] CrearReservaPorPeriodoRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _reservaService.CrearReservaPorPeriodoAsync(request, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Crea una reservación por jornada completa de 1 a 7 días dentro de una ventana máxima de 7 días.
    /// </summary>
    [HttpPost("por-jornada")]
    public async Task<IActionResult> CrearPorJornada([FromBody] CrearReservaPorJornadaRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _reservaService.CrearReservaPorJornadaAsync(request, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Crea una reservación recurrente semestral (Padre-Hija) en los días seleccionados a la misma hora.
    /// </summary>
    [HttpPost("semestral")]
    public async Task<IActionResult> CrearSemestral([FromBody] CrearReservaSemestralRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _reservaService.CrearReservaSemestralAsync(request, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancela una reservación. Si el ID corresponde a una instancia hija, cancela solo ese día.
    /// Si corresponde a la cabecera Padre, cancela toda la serie semestral o de jornada.
    /// </summary>
    [HttpPost("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarReservaRequestDto? request, CancellationToken cancellationToken)
    {
        var dto = request ?? new CancelarReservaRequestDto();
        var result = await _reservaService.CancelarReservaAsync(id, UsuarioActualId, EsAdministrador, dto, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Permite a un Administrador reasignar cualquier reservación a otro profesor/responsable.
    /// </summary>
    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpPut("{id:int}/reasignar")]
    public async Task<IActionResult> Reasignar(int id, [FromBody] ReasignarReservaRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _reservaService.ReasignarReservaAsync(id, request, UsuarioActualId, EsAdministrador, cancellationToken);
        return HandleResult(result);
    }
}
