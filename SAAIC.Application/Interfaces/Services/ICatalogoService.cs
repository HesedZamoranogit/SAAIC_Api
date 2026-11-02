using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Catalogos;

namespace SAAIC.Application.Interfaces.Services;

public interface ICatalogoService
{
    Task<ApiResponse<PagedResult<CampusDto>>> ObtenerCampusesAsync(PagedRequest? pagination = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CampusDto>> CrearCampusAsync(CrearCampusRequestDto request, string usuarioId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CampusDto>> ActualizarCampusAsync(int id, ActualizarCampusRequestDto request, string usuarioId, CancellationToken cancellationToken = default);

    Task<ApiResponse<PagedResult<TipoEspacioDto>>> ObtenerTiposEspacioAsync(PagedRequest? pagination = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<TipoEspacioDto>> CrearTipoEspacioAsync(CrearTipoEspacioRequestDto request, string usuarioId, CancellationToken cancellationToken = default);
    Task<ApiResponse<TipoEspacioDto>> ActualizarTipoEspacioAsync(int id, ActualizarTipoEspacioRequestDto request, string usuarioId, CancellationToken cancellationToken = default);
}
