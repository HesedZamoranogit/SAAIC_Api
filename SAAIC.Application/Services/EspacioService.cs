using SAAIC.Application.Common.Exceptions;
using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Espacios;
using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Application.Mappings;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;
using SAAIC.Domain.Exceptions;

namespace SAAIC.Application.Services;

public class EspacioService : IEspacioService
{
    private readonly IEspacioRepository _espacioRepository;
    private readonly IRepository<Campus> _campusRepository;
    private readonly IRepository<TipoEspacio> _tipoEspacioRepository;

    public EspacioService(
        IEspacioRepository espacioRepository,
        IRepository<Campus> campusRepository,
        IRepository<TipoEspacio> tipoEspacioRepository)
    {
        _espacioRepository = espacioRepository;
        _campusRepository = campusRepository;
        _tipoEspacioRepository = tipoEspacioRepository;
    }

    public async Task<ApiResponse<PagedResult<EspacioDto>>> ObtenerConFiltrosAsync(
        int? idCampus = null,
        int? idTipoEspacio = null,
        EstadoEspacio? estado = null,
        PagedRequest? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = pagination?.PageNumber ?? 1;
        var pageSize = pagination?.PageSize ?? 10;

        var espaciosPaginados = await _espacioRepository.ObtenerConFiltrosPaginadoAsync(
            idCampus, idTipoEspacio, estado, soloActivos: true, pageNumber, pageSize, cancellationToken);

        var pagedDtos = espaciosPaginados.MapTo(e => e.ToDto());
        return ApiResponse<PagedResult<EspacioDto>>.SuccessResult(pagedDtos);
    }

    public async Task<ApiResponse<EspacioDto>> ObtenerPorIdAsync(int idEspacio, CancellationToken cancellationToken = default)
    {
        var espacio = await _espacioRepository.ObtenerConDetallesPorIdAsync(idEspacio, cancellationToken)
            ?? throw new NotFoundException(nameof(Espacio), idEspacio);

        return ApiResponse<EspacioDto>.SuccessResult(espacio.ToDto());
    }

    public async Task<ApiResponse<EspacioDto>> CrearEspacioAsync(
        CrearEspacioRequestDto request,
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        ValidarParametrosEspacio(request.Nombre, request.HoraApertura, request.HoraCierre, request.CapacidadMaxima);

        var campus = await _campusRepository.GetByIdAsync(request.IdCampus, cancellationToken)
            ?? throw new NotFoundException(nameof(Campus), request.IdCampus);

        var tipo = await _tipoEspacioRepository.GetByIdAsync(request.IdTipoEspacio, cancellationToken)
            ?? throw new NotFoundException(nameof(TipoEspacio), request.IdTipoEspacio);

        var nombreLimpio = request.Nombre.Trim();
        var duplicados = await _espacioRepository.FindAsync(
            e => e.IdCampus == request.IdCampus && e.Nombre.ToLower() == nombreLimpio.ToLower(),
            cancellationToken);

        if (duplicados.Count > 0)
        {
            throw new DomainException($"Ya existe un espacio llamado '{nombreLimpio}' en el campus '{campus.Nombre}'.");
        }

        var nuevoEspacio = new Espacio
        {
            Nombre = nombreLimpio,
            IdTipoEspacio = tipo.Id,
            IdCampus = campus.Id,
            HoraApertura = request.HoraApertura,
            HoraCierre = request.HoraCierre,
            CapacidadMaxima = request.CapacidadMaxima,
            Estado = request.Estado,
            Active = true,
            CreatedBy = usuarioId,
            Campus = campus,
            TipoEspacio = tipo
        };

        await _espacioRepository.AddAsync(nuevoEspacio, cancellationToken);
        return ApiResponse<EspacioDto>.SuccessResult(nuevoEspacio.ToDto(), "Espacio creado exitosamente.");
    }

    public async Task<ApiResponse<EspacioDto>> ActualizarEspacioAsync(
        int idEspacio,
        ActualizarEspacioRequestDto request,
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        var espacio = await _espacioRepository.ObtenerConDetallesPorIdAsync(idEspacio, cancellationToken)
            ?? throw new NotFoundException(nameof(Espacio), idEspacio);

        ValidarParametrosEspacio(request.Nombre, request.HoraApertura, request.HoraCierre, request.CapacidadMaxima);

        var campus = await _campusRepository.GetByIdAsync(request.IdCampus, cancellationToken)
            ?? throw new NotFoundException(nameof(Campus), request.IdCampus);

        var tipo = await _tipoEspacioRepository.GetByIdAsync(request.IdTipoEspacio, cancellationToken)
            ?? throw new NotFoundException(nameof(TipoEspacio), request.IdTipoEspacio);

        espacio.Nombre = request.Nombre.Trim();
        espacio.IdTipoEspacio = tipo.Id;
        espacio.IdCampus = campus.Id;
        espacio.HoraApertura = request.HoraApertura;
        espacio.HoraCierre = request.HoraCierre;
        espacio.CapacidadMaxima = request.CapacidadMaxima;
        espacio.Estado = request.Estado;
        espacio.Active = request.Active;
        espacio.UpdatedBy = usuarioId;
        espacio.Campus = campus;
        espacio.TipoEspacio = tipo;

        await _espacioRepository.UpdateAsync(espacio, cancellationToken);
        return ApiResponse<EspacioDto>.SuccessResult(espacio.ToDto(), "Espacio actualizado exitosamente.");
    }

    public async Task<ApiResponse<EspacioDto>> CambiarEstadoAsync(
        int idEspacio,
        CambiarEstadoEspacioRequestDto request,
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        var espacio = await _espacioRepository.ObtenerConDetallesPorIdAsync(idEspacio, cancellationToken)
            ?? throw new NotFoundException(nameof(Espacio), idEspacio);

        espacio.Estado = request.NuevoEstado;
        espacio.UpdatedBy = usuarioId;

        await _espacioRepository.UpdateAsync(espacio, cancellationToken);
        return ApiResponse<EspacioDto>.SuccessResult(espacio.ToDto(), $"El estado del espacio cambió a '{request.NuevoEstado}'.");
    }

    private static void ValidarParametrosEspacio(string nombre, TimeOnly apertura, TimeOnly cierre, int capacidad)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del espacio es obligatorio.");

        if (apertura >= cierre)
            throw new DomainException("La hora de apertura debe ser estrictamente menor a la hora de cierre.");

        if (capacidad <= 0)
            throw new DomainException("La capacidad máxima del espacio debe ser mayor a cero.");
    }
}
