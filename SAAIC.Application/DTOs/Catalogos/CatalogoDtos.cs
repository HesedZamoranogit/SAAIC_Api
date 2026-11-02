namespace SAAIC.Application.DTOs.Catalogos;

public record CampusDto(
    int Id,
    string Nombre,
    string? Codigo,
    string? Direccion,
    bool Active);

public record CrearCampusRequestDto(
    string Nombre,
    string? Codigo,
    string? Direccion);

public record ActualizarCampusRequestDto(
    string Nombre,
    string? Codigo,
    string? Direccion,
    bool Active = true);

public record TipoEspacioDto(
    int Id,
    string Nombre,
    string? Descripcion,
    bool Active);

public record CrearTipoEspacioRequestDto(
    string Nombre,
    string? Descripcion);

public record ActualizarTipoEspacioRequestDto(
    string Nombre,
    string? Descripcion,
    bool Active = true);
