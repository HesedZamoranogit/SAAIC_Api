using System.Data;
using Microsoft.AspNetCore.Identity;
using SAAIC.Application.Common.Exceptions;
using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Reservas;
using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Application.Mappings;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;
using SAAIC.Domain.Exceptions;

namespace SAAIC.Application.Services;

public class ReservaService : IReservaService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IEspacioRepository _espacioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReservaService(
        IReservaRepository reservaRepository,
        IEspacioRepository espacioRepository,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _reservaRepository = reservaRepository;
        _espacioRepository = espacioRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<ApiResponse<ReservaDto>> CrearReservaPorPeriodoAsync(
        CrearReservaPorPeriodoRequestDto request,
        string idUsuarioActual,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            // 1. Bloqueo pesimista (UPDLOCK, ROWLOCK) sobre el espacio para prevenir Race Conditions
            var espacio = await _espacioRepository.ObtenerConBloqueoPesimistaAsync(request.IdEspacio, cancellationToken)
                ?? throw new NotFoundException(nameof(Espacio), request.IdEspacio);

            // 2. Creación de dominio (valida estado del espacio, capacidad estricta y horario operativo)
            var nuevaReserva = Reserva.CrearPorPeriodo(
                espacio,
                idUsuarioActual,
                request.ResponsableEvento,
                request.Descripcion,
                request.MaximoAsistentes,
                request.Fecha,
                request.HoraInicio,
                request.HoraFin,
                request.NotaSeguridad);

            // 3. Validación atómica de solapamiento de horarios dentro de la transacción bloqueada
            var existeChoque = await _reservaRepository.ExisteConflictoHorarioAsync(
                espacio.Id,
                request.Fecha,
                request.HoraInicio,
                request.HoraFin,
                excluirIdReserva: null,
                cancellationToken);

            if (existeChoque)
            {
                throw new DomainException(
                    $"El espacio '{espacio.Nombre}' ya se encuentra reservado el día {request.Fecha:yyyy-MM-dd} dentro del horario {request.HoraInicio:HH:mm} - {request.HoraFin:HH:mm}.");
            }

            await _reservaRepository.AddAsync(nuevaReserva, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var reservaCompleta = await _reservaRepository.ObtenerConDetallesPorIdAsync(nuevaReserva.Id, cancellationToken)
                ?? nuevaReserva;

            return ApiResponse<ReservaDto>.SuccessResult(
                reservaCompleta.ToDto(),
                "Reservación por periodo asignada exitosamente.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ApiResponse<ReservaDto>> CrearReservaPorJornadaAsync(
        CrearReservaPorJornadaRequestDto request,
        string idUsuarioActual,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            // 1. Bloqueo pesimista (UPDLOCK, ROWLOCK) sobre el espacio
            var espacio = await _espacioRepository.ObtenerConBloqueoPesimistaAsync(request.IdEspacio, cancellationToken)
                ?? throw new NotFoundException(nameof(Espacio), request.IdEspacio);

            // 2. Construcción y validación de rango de 1 a 7 días máximo en el dominio
            Reserva nuevaReserva;
            if (request.FechasSeleccionadas != null && request.FechasSeleccionadas.Count > 0)
            {
                nuevaReserva = Reserva.CrearPorJornada(
                    espacio,
                    idUsuarioActual,
                    request.ResponsableEvento,
                    request.Descripcion,
                    request.MaximoAsistentes,
                    request.FechasSeleccionadas,
                    request.NotaSeguridad);
            }
            else if (request.FechaInicio.HasValue && request.FechaFin.HasValue)
            {
                nuevaReserva = Reserva.CrearPorJornada(
                    espacio,
                    idUsuarioActual,
                    request.ResponsableEvento,
                    request.Descripcion,
                    request.MaximoAsistentes,
                    request.FechaInicio.Value,
                    request.FechaFin.Value,
                    request.NotaSeguridad);
            }
            else if (request.FechaInicio.HasValue)
            {
                nuevaReserva = Reserva.CrearPorJornada(
                    espacio,
                    idUsuarioActual,
                    request.ResponsableEvento,
                    request.Descripcion,
                    request.MaximoAsistentes,
                    new[] { request.FechaInicio.Value },
                    request.NotaSeguridad);
            }
            else
            {
                throw new DomainException("Debe proporcionar FechasSeleccionadas o FechaInicio/FechaFin para reservar por jornada.");
            }

            // 3. Verificar disponibilidad para todas las fechas seleccionadas
            var fechasAValidar = nuevaReserva.EsReservaPadre
                ? nuevaReserva.ReservasHijas.Select(h => h.FechaInicio).ToList()
                : new List<DateOnly> { nuevaReserva.FechaInicio };

            var fechasConConflicto = await _reservaRepository.ObtenerFechasConConflictoAsync(
                espacio.Id,
                fechasAValidar,
                espacio.HoraApertura,
                espacio.HoraCierre,
                cancellationToken);

            if (fechasConConflicto.Count > 0)
            {
                var listaFechasTexto = string.Join(", ", fechasConConflicto.Select(f => f.ToString("yyyy-MM-dd")));
                throw new DomainException(
                    $"No se puede reservar la jornada completa en '{espacio.Nombre}' porque ya existen eventos registrados en las siguientes fechas: {listaFechasTexto}.");
            }

            await _reservaRepository.AddAsync(nuevaReserva, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var reservaCompleta = await _reservaRepository.ObtenerConDetallesPorIdAsync(nuevaReserva.Id, cancellationToken)
                ?? nuevaReserva;

            return ApiResponse<ReservaDto>.SuccessResult(
                reservaCompleta.ToDto(),
                $"Reservación por jornada asignada exitosamente ({fechasAValidar.Count} día(s)).");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ApiResponse<ReservaDto>> CrearReservaSemestralAsync(
        CrearReservaSemestralRequestDto request,
        string idUsuarioActual,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            // 1. Bloqueo pesimista (UPDLOCK, ROWLOCK) sobre el espacio
            var espacio = await _espacioRepository.ObtenerConBloqueoPesimistaAsync(request.IdEspacio, cancellationToken)
                ?? throw new NotFoundException(nameof(Espacio), request.IdEspacio);

            // 2. Creación de la reserva Padre y proyección de todas las instancias Hijas del semestre
            var reservaPadre = Reserva.CrearRecurrenteSemestral(
                espacio,
                idUsuarioActual,
                request.ResponsableEvento,
                request.Descripcion,
                request.MaximoAsistentes,
                request.FechaInicioSemestre,
                request.FechaFinSemestre,
                request.HoraInicio,
                request.HoraFin,
                request.DiasSeleccionados,
                request.NotaSeguridad);

            // 3. Validar en bloque si alguna de las fechas proyectadas del semestre choca con otra reserva
            var fechasProyectadas = reservaPadre.ReservasHijas.Select(h => h.FechaInicio).ToList();

            var fechasOcupadas = await _reservaRepository.ObtenerFechasConConflictoAsync(
                espacio.Id,
                fechasProyectadas,
                request.HoraInicio,
                request.HoraFin,
                cancellationToken);

            if (fechasOcupadas.Count > 0)
            {
                var muestraFechas = string.Join(", ", fechasOcupadas.Take(10).Select(f => f.ToString("yyyy-MM-dd")));
                var sufijo = fechasOcupadas.Count > 10 ? $" (y {fechasOcupadas.Count - 10} más)" : string.Empty;

                throw new DomainException(
                    $"No se pudo asignar la reserva semestral en '{espacio.Nombre}' ({request.HoraInicio:HH:mm} - {request.HoraFin:HH:mm}) por choque de horario en las fechas: {muestraFechas}{sufijo}.");
            }

            await _reservaRepository.AddAsync(reservaPadre, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var reservaCompleta = await _reservaRepository.ObtenerConDetallesPorIdAsync(reservaPadre.Id, cancellationToken)
                ?? reservaPadre;

            return ApiResponse<ReservaDto>.SuccessResult(
                reservaCompleta.ToDto(),
                $"Reservación semestral creada exitosamente con {reservaPadre.ReservasHijas.Count} sesiones programadas.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ApiResponse<ReservaDto>> CancelarReservaAsync(
        int idReserva,
        string idUsuarioEjecutor,
        bool esAdministrador,
        CancelarReservaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var reserva = await _reservaRepository.ObtenerConDetallesPorIdAsync(idReserva, cancellationToken)
            ?? throw new NotFoundException(nameof(Reserva), idReserva);

        reserva.Cancelar(idUsuarioEjecutor, esAdministrador, request.MotivoCancelacion);

        await _reservaRepository.UpdateAsync(reserva, cancellationToken);

        var mensaje = reserva.EsReservaPadre
            ? "La reservación completa y todas sus sesiones pendientes han sido canceladas."
            : "La reservación (sesión del día) ha sido cancelada exitosamente.";

        return ApiResponse<ReservaDto>.SuccessResult(reserva.ToDto(), mensaje);
    }

    public async Task<ApiResponse<ReservaDto>> ReasignarReservaAsync(
        int idReserva,
        ReasignarReservaRequestDto request,
        string adminEjecutorId,
        bool esAdministrador,
        CancellationToken cancellationToken = default)
    {
        var reserva = await _reservaRepository.ObtenerConDetallesPorIdAsync(idReserva, cancellationToken)
            ?? throw new NotFoundException(nameof(Reserva), idReserva);

        var nuevoUsuario = await _userManager.FindByIdAsync(request.NuevoIdUsuario)
            ?? throw new NotFoundException(nameof(ApplicationUser), request.NuevoIdUsuario);

        if (!nuevoUsuario.Active)
            throw new DomainException("No se puede reasignar la reservación a un usuario inactivo.");

        reserva.Reasignar(nuevoUsuario.Id, request.NuevoResponsableEvento, esAdministrador, adminEjecutorId);

        await _reservaRepository.UpdateAsync(reserva, cancellationToken);

        return ApiResponse<ReservaDto>.SuccessResult(
            reserva.ToDto(),
            $"Reservación reasignada exitosamente a '{nuevoUsuario.NombreCompleto}'.");
    }

    public async Task<ApiResponse<ReservaDto>> ObtenerPorIdAsync(
        int idReserva,
        CancellationToken cancellationToken = default)
    {
        var reserva = await _reservaRepository.ObtenerConDetallesPorIdAsync(idReserva, cancellationToken)
            ?? throw new NotFoundException(nameof(Reserva), idReserva);

        return ApiResponse<ReservaDto>.SuccessResult(reserva.ToDto());
    }

    public async Task<ApiResponse<IReadOnlyList<ReservaInstanciaDiariaDto>>> ObtenerCalendarioInstanciasAsync(
        DateOnly fechaInicio,
        DateOnly fechaFin,
        int? idCampus = null,
        int? idEspacio = null,
        CancellationToken cancellationToken = default)
    {
        if (fechaFin < fechaInicio)
            throw new DomainException("La fecha de fin no puede ser menor a la fecha de inicio.");

        var instancias = await _reservaRepository.ObtenerInstanciasPorRangoFechasAsync(
            fechaInicio,
            fechaFin,
            idCampus,
            idEspacio,
            soloConfirmadas: true,
            cancellationToken);

        var dtos = instancias.Select(i => i.ToInstanciaDto()).ToList();
        return ApiResponse<IReadOnlyList<ReservaInstanciaDiariaDto>>.SuccessResult(dtos);
    }

    public async Task<ApiResponse<PagedResult<ReservaDto>>> ObtenerMisReservasAsync(
        string idUsuario,
        PagedRequest? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = pagination?.PageNumber ?? 1;
        var pageSize = pagination?.PageSize ?? 10;

        var reservasPaginadas = await _reservaRepository.ObtenerPorUsuarioPaginadoAsync(
            idUsuario, pageNumber, pageSize, cancellationToken);

        var pagedDtos = reservasPaginadas.MapTo(r => r.ToDto());
        return ApiResponse<PagedResult<ReservaDto>>.SuccessResult(pagedDtos);
    }

    public async Task<ApiResponse<PagedResult<ReservaDto>>> ObtenerTodasAsync(
        int? idCampus = null,
        int? idEspacio = null,
        string? idUsuario = null,
        EstadoReserva? estado = null,
        DateOnly? fechaInicio = null,
        DateOnly? fechaFin = null,
        PagedRequest? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = pagination?.PageNumber ?? 1;
        var pageSize = pagination?.PageSize ?? 10;

        var reservasPaginadas = await _reservaRepository.ObtenerTodasPaginadoAsync(
            idCampus, idEspacio, idUsuario, estado, fechaInicio, fechaFin, pageNumber, pageSize, cancellationToken);

        var pagedDtos = reservasPaginadas.MapTo(r => r.ToDto());
        return ApiResponse<PagedResult<ReservaDto>>.SuccessResult(pagedDtos);
    }
}
