using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Espacios;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Domain.Common;
using SAAIC.Domain.Enums;

namespace SAAIC.Web.Controllers;

[Authorize]
public class EspaciosController : BaseApiController
{
    private readonly IEspacioService _espacioService;

    public EspaciosController(IEspacioService espacioService)
    {
        _espacioService = espacioService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerEspacios(
        [FromQuery] int? idCampus = null,
        [FromQuery] int? idTipoEspacio = null,
        [FromQuery] EstadoEspacio? estado = null,
        [FromQuery] PagedRequest? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _espacioService.ObtenerConFiltrosAsync(idCampus, idTipoEspacio, estado, pagination, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var result = await _espacioService.ObtenerPorIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpPost]
    public async Task<IActionResult> CrearEspacio([FromBody] CrearEspacioRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _espacioService.CrearEspacioAsync(request, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarEspacio(int id, [FromBody] ActualizarEspacioRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _espacioService.ActualizarEspacioAsync(id, request, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = RolesSistema.Administrador)]
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoEspacioRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _espacioService.CambiarEstadoAsync(id, request, UsuarioActualId, cancellationToken);
        return HandleResult(result);
    }
}
