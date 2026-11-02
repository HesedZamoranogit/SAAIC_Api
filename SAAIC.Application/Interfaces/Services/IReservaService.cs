using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Reservas;
using SAAIC.Domain.Enums;

namespace SAAIC.Application.Interfaces.Services;

public interface IReservaService
{
    Task<ApiResponse<ReservaDto>> CrearReservaPorPeriodoAsync(
        CrearReservaPorPeriodoRequestDto request,
        string idUsuarioActual,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ReservaDto>> CrearReservaPorJornadaAsync(
        CrearReservaPorJornadaRequestDto request,
        string idUsuarioActual,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ReservaDto>> CrearReservaSemestralAsync(
        CrearReservaSemestralRequestDto request,
        string idUsuarioActual,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ReservaDto>> CancelarReservaAsync(
        int idReserva,
        string idUsuarioEjecutor,
        bool esAdministrador,
        CancelarReservaRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ReservaDto>> ReasignarReservaAsync(
        int idReserva,
        ReasignarReservaRequestDto request,
        string adminEjecutorId,
        bool esAdministrador,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ReservaDto>> ObtenerPorIdAsync(
        int idReserva,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<ReservaInstanciaDiariaDto>>> ObtenerCalendarioInstanciasAsync(
        DateOnly fechaInicio,
        DateOnly fechaFin,
        int? idCampus = null,
        int? idEspacio = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PagedResult<ReservaDto>>> ObtenerMisReservasAsync(
        string idUsuario,
        PagedRequest? pagination = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PagedResult<ReservaDto>>> ObtenerTodasAsync(
        int? idCampus = null,
        int? idEspacio = null,
        string? idUsuario = null,
        EstadoReserva? estado = null,
        DateOnly? fechaInicio = null,
        DateOnly? fechaFin = null,
        PagedRequest? pagination = null,
        CancellationToken cancellationToken = default);
}
