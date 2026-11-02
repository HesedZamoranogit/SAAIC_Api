using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SAAIC.Application.Common.Models;
using SAAIC.Domain.Common;
using SAAIC.Domain.Exceptions;

namespace SAAIC.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Obtiene el ID del usuario autenticado desde los Claims del JWT.
    /// </summary>
    protected string UsuarioActualId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new DomainException("No se pudo identificar al usuario autenticado en el token.");

    /// <summary>
    /// Indica si el usuario autenticado posee el rol de Administrador.
    /// </summary>
    protected bool EsAdministrador => User.IsInRole(RolesSistema.Administrador);

    protected ActionResult HandleResult<T>(ApiResponse<T> result)
    {
        if (result == null)
            return NotFound();

        if (result.Success && result.Data != null)
            return Ok(result);

        if (result.Success && result.Data == null)
            return NotFound(result);

        return BadRequest(result);
    }
}
