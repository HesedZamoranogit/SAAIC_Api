using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Auth;

namespace SAAIC.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> RegistrarUsuarioAsync(RegisterUserRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> SolicitarRecuperacionPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> RestablecerPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<UserDto>>> ListarUsuariosAsync(
        string? rol = null,
        string? busqueda = null,
        PagedRequest? pagination = null,
        CancellationToken cancellationToken = default);
}
