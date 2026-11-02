using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Catalogos;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Domain.Common;

namespace SAAIC.Web.Controllers;

[Authorize]
public class CatalogosController : BaseApiController
{
    private readonly ICatalogoService _catalogoService;

    public CatalogosController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [HttpGet("campuses")]
    public async Task<IActionResult> ObtenerCampuses([FromQuery] PagedRequest? pagination, CancellationToken cancellationToken)
    {
        var result = await _catalogoService.ObtenerCampusesAsync(pagination, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpPost("campuses")]
    public async Task<IActionResult> CrearCampus([FromBody] CrearCampusRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _catalogoService.CrearCampusAsync(request, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpPut("campuses/{id:int}")]
    public async Task<IActionResult> ActualizarCampus(int id, [FromBody] ActualizarCampusRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _catalogoService.ActualizarCampusAsync(id, request, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("tipos-espacio")]
    public async Task<IActionResult> ObtenerTiposEspacio([FromQuery] PagedRequest? pagination, CancellationToken cancellationToken)
    {
        var result = await _catalogoService.ObtenerTiposEspacioAsync(pagination, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpPost("tipos-espacio")]
    public async Task<IActionResult> CrearTipoEspacio([FromBody] CrearTipoEspacioRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _catalogoService.CrearTipoEspacioAsync(request, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpPut("tipos-espacio/{id:int}")]
    public async Task<IActionResult> ActualizarTipoEspacio(int id, [FromBody] ActualizarTipoEspacioRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _catalogoService.ActualizarTipoEspacioAsync(id, request, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }
}
