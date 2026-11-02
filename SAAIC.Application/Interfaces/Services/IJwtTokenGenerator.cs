using SAAIC.Domain.Entities;

namespace SAAIC.Application.Interfaces.Services;

public interface IJwtTokenGenerator
{
    /// <summary>
    /// Genera un token JWT firmado para el usuario autenticado junto con su fecha de expiración.
    /// </summary>
    (string Token, DateTime ExpiresAtUtc) GenerateToken(ApplicationUser user, IEnumerable<string> roles);
}
