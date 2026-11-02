using SAAIC.Domain.Common;

namespace SAAIC.Application.DTOs.Auth;

public record LoginRequestDto(
    string IdInstitucional,
    string Password);

public record RegisterUserRequestDto(
    string IdInstitucional,
    string Email,
    string Password,
    string NombreCompleto,
    string? Departamento,
    string Rol = RolesSistema.Profesor);

public record AuthResponseDto(
    string Token,
    DateTime ExpiresAtUtc,
    string UserId,
    string Email,
    string NombreCompleto,
    string IdInstitucional,
    IReadOnlyList<string> Roles);

public record ForgotPasswordRequestDto(
    string Email);

public record ResetPasswordRequestDto(
    string Email,
    string Token,
    string NewPassword);

public record UserDto(
    string Id,
    string Email,
    string NombreCompleto,
    string IdInstitucional,
    string? Departamento,
    bool Active,
    IReadOnlyList<string> Roles);
