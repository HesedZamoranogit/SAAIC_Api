using SAAIC.Application.DTOs.Asistencias;
using SAAIC.Application.DTOs.Catalogos;
using SAAIC.Application.DTOs.Espacios;
using SAAIC.Application.DTOs.Reservas;
using SAAIC.Domain.Entities;

namespace SAAIC.Application.Mappings;

public static class EntityMappingExtensions
{
    public static CampusDto ToDto(this Campus campus) => new(
        campus.Id,
        campus.Nombre,
        campus.Codigo,
        campus.Direccion,
        campus.Active);

    public static TipoEspacioDto ToDto(this TipoEspacio tipoEspacio) => new(
        tipoEspacio.Id,
        tipoEspacio.Nombre,
        tipoEspacio.Descripcion,
        tipoEspacio.Active);

    public static EspacioDto ToDto(this Espacio espacio) => new(
        espacio.Id,
        espacio.Nombre,
        espacio.IdTipoEspacio,
        espacio.TipoEspacio?.Nombre ?? string.Empty,
        espacio.IdCampus,
        espacio.Campus?.Nombre ?? string.Empty,
        espacio.HoraApertura,
        espacio.HoraCierre,
        espacio.Estado,
        espacio.Estado.ToString(),
        espacio.CapacidadMaxima,
        espacio.Active);

    public static AsistenciaDto ToDto(this Asistencia asistencia) => new(
        asistencia.Id,
        asistencia.IdReserva,
        asistencia.IdUsuario,
        asistencia.Usuario?.NombreCompleto ?? string.Empty,
        asistencia.FechaEvento,
        asistencia.HoraProgramadaInicio,
        asistencia.HoraProgramadaFin,
        asistencia.HoraEntrada,
        asistencia.HoraSalida,
        asistencia.Estado,
        asistencia.Estado.ToString(),
        asistencia.MinutosRetardo,
        asistencia.MinutosToleranciaAplicados,
        asistencia.MarcoEntrada,
        asistencia.MarcoSalida,
        asistencia.Observaciones);

    public static ReservaInstanciaDiariaDto ToInstanciaDto(this Reserva reserva) => new(
        reserva.Id,
        reserva.IdReservaPadre,
        reserva.IdEspacio,
        reserva.Espacio?.Nombre ?? string.Empty,
        reserva.Espacio?.TipoEspacio?.Nombre ?? string.Empty,
        reserva.IdCampus,
        reserva.Campus?.Nombre ?? string.Empty,
        reserva.IdUsuario,
        reserva.Usuario?.NombreCompleto ?? string.Empty,
        reserva.ResponsableEvento,
        reserva.Descripcion,
        reserva.MaximoAsistentes,
        reserva.NotaSeguridad,
        reserva.TipoReserva,
        reserva.Estado,
        reserva.FechaInicio,
        reserva.HoraInicio,
        reserva.HoraFin,
        reserva.Asistencia?.ToDto());

    public static ReservaDto ToDto(this Reserva reserva) => new(
        reserva.Id,
        reserva.IdReservaPadre,
        reserva.EsReservaPadre,
        reserva.IdEspacio,
        reserva.Espacio?.Nombre ?? string.Empty,
        reserva.Espacio?.TipoEspacio?.Nombre ?? string.Empty,
        reserva.IdCampus,
        reserva.Campus?.Nombre ?? string.Empty,
        reserva.IdUsuario,
        reserva.Usuario?.NombreCompleto ?? string.Empty,
        reserva.ResponsableEvento,
        reserva.Descripcion,
        reserva.MaximoAsistentes,
        reserva.NotaSeguridad,
        reserva.TipoReserva,
        reserva.TipoReserva.ToString(),
        reserva.Estado,
        reserva.Estado.ToString(),
        reserva.FechaInicio,
        reserva.FechaFin,
        reserva.HoraInicio,
        reserva.HoraFin,
        reserva.DiasRecurrencia,
        reserva.FechaCancelacion,
        reserva.MotivoCancelacion,
        reserva.Asistencia?.ToDto(),
        reserva.ReservasHijas
            .OrderBy(h => h.FechaInicio)
            .Select(h => h.ToInstanciaDto())
            .ToList());

    public static ConfiguracionSistemaDto ToDto(this ConfiguracionSistema config) => new(
        config.Id,
        config.MinutosToleranciaRetardo,
        config.MinutosAnticipacionCheckIn,
        config.EnvioAutomaticoReportesActivo,
        config.HoraEnvioReporteEventos,
        config.HoraEnvioReporteAsistencia,
        config.CorreosDestinatariosReportes);
}
