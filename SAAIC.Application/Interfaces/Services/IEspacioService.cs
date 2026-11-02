using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Espacios;
using SAAIC.Domain.Enums;

namespace SAAIC.Application.Interfaces.Services;

public interface IEspacioService
{
    Task<ApiResponse<PagedResult<EspacioDto>>> ObtenerConFiltrosAsync(
        int? idCampus = null,
        int? idTipoEspacio = null,
        EstadoEspacio? estado = null,
        PagedRequest? pagination = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<EspacioDto>> ObtenerPorIdAsync(int idEspacio, CancellationToken cancellationToken = default);

    Task<ApiResponse<EspacioDto>> CrearEspacioAsync(
        CrearEspacioRequestDto request,
        string usuarioId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<EspacioDto>> ActualizarEspacioAsync(
        int idEspacio,
        ActualizarEspacioRequestDto request,
        string usuarioId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<EspacioDto>> CambiarEstadoAsync(
        int idEspacio,
        CambiarEstadoEspacioRequestDto request,
        string usuarioId,
        CancellationToken cancellationToken = default);
}
