using SAAIC.Domain.Enums;

namespace SAAIC.Application.DTOs.Espacios;

public record EspacioDto(
    int Id,
    string Nombre,
    int IdTipoEspacio,
    string NombreTipoEspacio,
    int IdCampus,
    string NombreCampus,
    TimeOnly HoraApertura,
    TimeOnly HoraCierre,
    EstadoEspacio Estado,
    string EstadoDescripcion,
    int CapacidadMaxima,
    bool Active);

public record CrearEspacioRequestDto(
    string Nombre,
    int IdTipoEspacio,
    int IdCampus,
    TimeOnly HoraApertura,
    TimeOnly HoraCierre,
    int CapacidadMaxima,
    EstadoEspacio Estado = EstadoEspacio.Disponible);

public record ActualizarEspacioRequestDto(
    string Nombre,
    int IdTipoEspacio,
    int IdCampus,
    TimeOnly HoraApertura,
    TimeOnly HoraCierre,
    int CapacidadMaxima,
    EstadoEspacio Estado,
    bool Active = true);

public record CambiarEstadoEspacioRequestDto(
    EstadoEspacio NuevoEstado);
