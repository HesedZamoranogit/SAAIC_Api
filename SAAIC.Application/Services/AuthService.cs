using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Auth;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Domain.Common;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Exceptions;

namespace SAAIC.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailService emailService)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdInstitucional) && string.IsNullOrWhiteSpace(request.Password))
        {
            throw new DomainException("Debe ingresar el ID Institucional y la contraseña.");
        }

        if (string.IsNullOrWhiteSpace(request.IdInstitucional))
        {
            throw new DomainException("El ID Institucional es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new DomainException("La contraseña es obligatoria.");
        }

        var idInstitucional = request.IdInstitucional.Trim();
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.IdInstitucional == idInstitucional, cancellationToken);

        if (user == null || !user.Active)
        {
            throw new DomainException("Credenciales inválidas.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            throw new DomainException("Credenciales inválidas.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user, roles);

        var response = new AuthResponseDto(
            token,
            expiresAtUtc,
            user.Id,
            user.Email ?? string.Empty,
            user.NombreCompleto,
            user.IdInstitucional,
            roles.ToList());

        return ApiResponse<AuthResponseDto>.SuccessResult(response, "Inicio de sesión exitoso.");
    }

    public async Task<ApiResponse<UserDto>> RegistrarUsuarioAsync(
        RegisterUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdInstitucional) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.NombreCompleto))
        {
            throw new DomainException("El ID Institucional, el correo electrónico y el nombre completo son obligatorios.");
        }

        var idInstitucional = request.IdInstitucional.Trim();
        var emailLimpio = request.Email.Trim();

        var rolAsignar = string.IsNullOrWhiteSpace(request.Rol)
            ? RolesSistema.Profesor
            : request.Rol.Trim();

        if (rolAsignar != RolesSistema.Profesor && rolAsignar != RolesSistema.Administrador)
        {
            throw new DomainException($"El rol '{rolAsignar}' no es válido. Use '{RolesSistema.Profesor}' o '{RolesSistema.Administrador}'.");
        }

        var idDuplicado = await _userManager.Users.AnyAsync(u => u.IdInstitucional == idInstitucional, cancellationToken);
        if (idDuplicado)
        {
            throw new DomainException($"Ya existe un usuario registrado con el ID Institucional '{idInstitucional}'.");
        }

        var existingUser = await _userManager.FindByEmailAsync(emailLimpio);
        if (existingUser != null)
        {
            throw new DomainException($"Ya existe un usuario registrado con el correo '{emailLimpio}'.");
        }

        var nuevoUsuario = new ApplicationUser
        {
            UserName = idInstitucional,
            Email = emailLimpio,
            EmailConfirmed = true,
            NombreCompleto = request.NombreCompleto.Trim(),
            IdInstitucional = idInstitucional,
            Departamento = request.Departamento?.Trim(),
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(nuevoUsuario, request.Password);
        if (!createResult.Succeeded)
        {
            var errores = createResult.Errors.Select(e => e.Description).ToList();
            return ApiResponse<UserDto>.FailureResult("No se pudo registrar el usuario.", errores);
        }

        await _userManager.AddToRoleAsync(nuevoUsuario, rolAsignar);

        var dto = new UserDto(
            nuevoUsuario.Id,
            nuevoUsuario.Email ?? string.Empty,
            nuevoUsuario.NombreCompleto,
            nuevoUsuario.IdInstitucional,
            nuevoUsuario.Departamento,
            nuevoUsuario.Active,
            new[] { rolAsignar });

        return ApiResponse<UserDto>.SuccessResult(dto, "Usuario registrado exitosamente.");
    }

    public async Task<ApiResponse<string>> SolicitarRecuperacionPasswordAsync(
        ForgotPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new DomainException("El correo electrónico es obligatorio.");
        }

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user == null || !user.Active)
        {
            // Por seguridad, respondemos mensaje genérico para no revelar existencia de cuentas
            return ApiResponse<string>.SuccessResult(
                "Si el correo existe en el sistema, se ha enviado un token de recuperación.",
                "Solicitud procesada.");
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; color: #333;'>
                <h2 style='color: #004B87;'>Recuperación de Contraseña - Sistema de Reservas ITSON</h2>
                <p>Hola <strong>{user.NombreCompleto}</strong>,</p>
                <p>Has solicitado restablecer tu contraseña. Utiliza el siguiente token de seguridad en el sistema:</p>
                <div style='background: #f4f6f9; padding: 12px; border-left: 4px solid #004B87; font-family: monospace; word-break: break-all;'>
                    {resetToken}
                </div>
                <p style='margin-top: 16px; font-size: 12px; color: #666;'>Si no solicitaste este cambio, puedes ignorar este mensaje.</p>
            </div>";

        await _emailService.SendEmailAsync(
            user.Email!,
            "Token de Recuperación de Contraseña - ITSON",
            htmlBody,
            cancellationToken);

        return ApiResponse<string>.SuccessResult(
            "Se ha enviado el token de recuperación a tu correo institucional.",
            "Token enviado exitosamente.");
    }

    public async Task<ApiResponse<string>> RestablecerPasswordAsync(
        ResetPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new DomainException("El correo, el token y la nueva contraseña son obligatorios.");
        }

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user == null || !user.Active)
        {
            throw new DomainException("No se pudo procesar el restablecimiento de contraseña.");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token.Trim(), request.NewPassword);
        if (!result.Succeeded)
        {
            var errores = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<string>.FailureResult("El token es inválido o la nueva contraseña no cumple con los requisitos.", errores);
        }

        return ApiResponse<string>.SuccessResult("Contraseña actualizada.", "Tu contraseña ha sido restablecida exitosamente.");
    }

    public async Task<ApiResponse<PagedResult<UserDto>>> ListarUsuariosAsync(
        string? rol = null,
        string? busqueda = null,
        PagedRequest? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = pagination?.PageNumber ?? 1;
        var pageSize = pagination?.PageSize ?? 10;

        List<ApplicationUser> usuarios;

        if (!string.IsNullOrWhiteSpace(rol))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(rol.Trim());
            usuarios = usersInRole.Where(u => u.Active).ToList();
        }
        else
        {
            usuarios = _userManager.Users.Where(u => u.Active).ToList();
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim().ToLower();
            usuarios = usuarios.Where(u =>
                (!string.IsNullOrEmpty(u.NombreCompleto) && u.NombreCompleto.ToLower().Contains(term)) ||
                (!string.IsNullOrEmpty(u.Email) && u.Email.ToLower().Contains(term)) ||
                (!string.IsNullOrEmpty(u.IdInstitucional) && u.IdInstitucional.ToLower().Contains(term))
            ).ToList();
        }

        usuarios = usuarios.OrderBy(u => u.NombreCompleto).ToList();

        var pagedUsers = usuarios.ToPagedResult(pageNumber, pageSize);

        var resultado = new List<UserDto>(pagedUsers.Items.Count);
        foreach (var u in pagedUsers.Items)
        {
            var roles = await _userManager.GetRolesAsync(u);
            resultado.Add(new UserDto(
                u.Id,
                u.Email ?? string.Empty,
                u.NombreCompleto,
                u.IdInstitucional,
                u.Departamento,
                u.Active,
                roles.ToList()));
        }

        var result = new PagedResult<UserDto>
        {
            Items = resultado,
            PageNumber = pagedUsers.PageNumber,
            PageSize = pagedUsers.PageSize,
            TotalRecords = pagedUsers.TotalRecords,
            TotalPages = pagedUsers.TotalPages
        };

        return ApiResponse<PagedResult<UserDto>>.SuccessResult(result);
    }
}
