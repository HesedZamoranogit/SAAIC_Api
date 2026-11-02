using SAAIC.Application.Common.Exceptions;
using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Catalogos;
using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Application.Mappings;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Exceptions;

namespace SAAIC.Application.Services;

public class CatalogoService : ICatalogoService
{
    private readonly IRepository<Campus> _campusRepository;
    private readonly IRepository<TipoEspacio> _tipoEspacioRepository;

    public CatalogoService(
        IRepository<Campus> campusRepository,
        IRepository<TipoEspacio> tipoEspacioRepository)
    {
        _campusRepository = campusRepository;
        _tipoEspacioRepository = tipoEspacioRepository;
    }

    public async Task<ApiResponse<PagedResult<CampusDto>>> ObtenerCampusesAsync(
        PagedRequest? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var campuses = await _campusRepository.FindAsync(c => c.Active, cancellationToken);
        var dtos = campuses.OrderBy(c => c.Nombre).Select(c => c.ToDto()).ToList();
        var paged = dtos.ToPagedResult(pagination?.PageNumber ?? 1, pagination?.PageSize ?? 0);
        return ApiResponse<PagedResult<CampusDto>>.SuccessResult(paged);
    }

    public async Task<ApiResponse<CampusDto>> CrearCampusAsync(
        CrearCampusRequestDto request,
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new DomainException("El nombre del campus es obligatorio.");

        var nombreLimpio = request.Nombre.Trim();
        var existentes = await _campusRepository.FindAsync(
            c => c.Nombre.ToLower() == nombreLimpio.ToLower(),
            cancellationToken);

        if (existentes.Count > 0)
            throw new DomainException($"Ya existe un campus con el nombre '{nombreLimpio}'.");

        var nuevoCampus = new Campus
        {
            Nombre = nombreLimpio,
            Codigo = request.Codigo?.Trim(),
            Direccion = request.Direccion?.Trim(),
            Active = true,
            CreatedBy = usuarioId
        };

        await _campusRepository.AddAsync(nuevoCampus, cancellationToken);
        return ApiResponse<CampusDto>.SuccessResult(nuevoCampus.ToDto(), "Campus creado exitosamente.");
    }

    public async Task<ApiResponse<CampusDto>> ActualizarCampusAsync(
        int id,
        ActualizarCampusRequestDto request,
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        var campus = await _campusRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Campus), id);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new DomainException("El nombre del campus es obligatorio.");

        campus.Nombre = request.Nombre.Trim();
        campus.Codigo = request.Codigo?.Trim();
        campus.Direccion = request.Direccion?.Trim();
        campus.Active = request.Active;
        campus.UpdatedBy = usuarioId;

        await _campusRepository.UpdateAsync(campus, cancellationToken);
        return ApiResponse<CampusDto>.SuccessResult(campus.ToDto(), "Campus actualizado exitosamente.");
    }

    public async Task<ApiResponse<PagedResult<TipoEspacioDto>>> ObtenerTiposEspacioAsync(
        PagedRequest? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var tipos = await _tipoEspacioRepository.FindAsync(t => t.Active, cancellationToken);
        var dtos = tipos.OrderBy(t => t.Nombre).Select(t => t.ToDto()).ToList();
        var paged = dtos.ToPagedResult(pagination?.PageNumber ?? 1, pagination?.PageSize ?? 0);
        return ApiResponse<PagedResult<TipoEspacioDto>>.SuccessResult(paged);
    }

    public async Task<ApiResponse<TipoEspacioDto>> CrearTipoEspacioAsync(
        CrearTipoEspacioRequestDto request,
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new DomainException("El nombre del tipo de espacio es obligatorio.");

        var nombreLimpio = request.Nombre.Trim();
        var existentes = await _tipoEspacioRepository.FindAsync(
            t => t.Nombre.ToLower() == nombreLimpio.ToLower(),
            cancellationToken);

        if (existentes.Count > 0)
            throw new DomainException($"Ya existe un tipo de espacio llamado '{nombreLimpio}'.");

        var nuevoTipo = new TipoEspacio
        {
            Nombre = nombreLimpio,
            Descripcion = request.Descripcion?.Trim(),
            Active = true,
            CreatedBy = usuarioId
        };

        await _tipoEspacioRepository.AddAsync(nuevoTipo, cancellationToken);
        return ApiResponse<TipoEspacioDto>.SuccessResult(nuevoTipo.ToDto(), "Tipo de espacio creado exitosamente.");
    }

    public async Task<ApiResponse<TipoEspacioDto>> ActualizarTipoEspacioAsync(
        int id,
        ActualizarTipoEspacioRequestDto request,
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        var tipo = await _tipoEspacioRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(TipoEspacio), id);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new DomainException("El nombre del tipo de espacio es obligatorio.");

        tipo.Nombre = request.Nombre.Trim();
        tipo.Descripcion = request.Descripcion?.Trim();
        tipo.Active = request.Active;
        tipo.UpdatedBy = usuarioId;

        await _tipoEspacioRepository.UpdateAsync(tipo, cancellationToken);
        return ApiResponse<TipoEspacioDto>.SuccessResult(tipo.ToDto(), "Tipo de espacio actualizado exitosamente.");
    }
}
