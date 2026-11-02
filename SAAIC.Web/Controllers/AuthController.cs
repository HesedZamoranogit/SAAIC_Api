using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Auth;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Domain.Common;

namespace SAAIC.Web.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Inicia sesión y emite un token JWT.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema (Profesor o Administrador).
    /// </summary>
    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpPost("registro")]
    public async Task<IActionResult> RegistrarUsuario([FromBody] RegisterUserRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegistrarUsuarioAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Envía el token de recuperación de contraseña al correo del usuario mediante la API de Gmail.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("recuperar-password")]
    public async Task<IActionResult> SolicitarRecuperacionPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.SolicitarRecuperacionPasswordAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Restablece la contraseña validando el token recibido por correo.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("restablecer-password")]
    public async Task<IActionResult> RestablecerPassword([FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.RestablecerPasswordAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene el listado de usuarios (Profesores y/o Administradores) de forma paginada con filtros.
    /// </summary>
    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpGet("usuarios")]
    public async Task<IActionResult> ListarUsuarios(
        [FromQuery] string? rol = null,
        [FromQuery] string? busqueda = null,
        [FromQuery] PagedRequest? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _authService.ListarUsuariosAsync(rol, busqueda, pagination, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene la información del perfil del usuario actualmente autenticado.
    /// </summary>
    [Authorize]
    [HttpGet("perfil")]
    public IActionResult ObtenerPerfilActual()
    {
        var perfil = new
        {
            UserId = UsuarioActualId,
            Email = User.FindFirstValue(ClaimTypes.Email),
            NombreCompleto = User.FindFirstValue(ClaimTypes.Name),
            IdInstitucional = User.FindFirstValue("IdInstitucional"),
            Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        };

        return Ok(ApiResponse<object>.SuccessResult(perfil));
    }
}
