using ClosedXML.Excel;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Domain.Entities;

namespace SAAIC.Infrastructure.Services;

/// <summary>
/// Implementación con ClosedXML para generar los reportes institucionales en formato Excel (.xlsx).
/// </summary>
public class ExcelReportGenerator : IExcelReportGenerator
{
    public byte[] GenerarReporteEventosYEspacios(
        IEnumerable<Reserva> instanciasEventos,
        IEnumerable<Espacio> espacios,
        DateOnly fechaInicio,
        DateOnly fechaFin)
    {
        var listaEventos = instanciasEventos.ToList();
        var listaEspacios = espacios.ToList();

        using var workbook = new XLWorkbook();

        // ================= HOJA 1: EVENTOS PROGRAMADOS =================
        var wsEventos = workbook.Worksheets.Add("Eventos Programados");

        string[] headersEventos =
        {
            "Fecha",
            "Hora Inicio",
            "Hora Fin",
            "Modalidad",
            "Espacio",
            "Tipo Espacio",
            "Campus",
            "Responsable del Evento",
            "Usuario Creador",
            "Correo Usuario",
            "Asistentes Esperados",
            "Nota de Seguridad",
            "Estado Reserva"
        };

        FormatearEncabezado(wsEventos, headersEventos);

        int fila = 2;
        foreach (var ev in listaEventos)
        {
            wsEventos.Cell(fila, 1).Value = ev.FechaInicio.ToString("yyyy-MM-dd");
            wsEventos.Cell(fila, 2).Value = ev.HoraInicio.ToString("HH:mm");
            wsEventos.Cell(fila, 3).Value = ev.HoraFin.ToString("HH:mm");
            wsEventos.Cell(fila, 4).Value = ev.TipoReserva.ToString();
            wsEventos.Cell(fila, 5).Value = ev.Espacio?.Nombre ?? $"Espacio #{ev.IdEspacio}";
            wsEventos.Cell(fila, 6).Value = ev.Espacio?.TipoEspacio?.Nombre ?? "N/A";
            wsEventos.Cell(fila, 7).Value = ev.Campus?.Nombre ?? $"Campus #{ev.IdCampus}";
            wsEventos.Cell(fila, 8).Value = ev.ResponsableEvento;
            wsEventos.Cell(fila, 9).Value = ev.Usuario?.NombreCompleto ?? ev.IdUsuario;
            wsEventos.Cell(fila, 10).Value = ev.Usuario?.Email ?? string.Empty;
            wsEventos.Cell(fila, 11).Value = ev.MaximoAsistentes;
            wsEventos.Cell(fila, 12).Value = ev.NotaSeguridad ?? string.Empty;
            wsEventos.Cell(fila, 13).Value = ev.Estado.ToString();
            fila++;
        }

        wsEventos.Columns().AdjustToContents();

        // ================= HOJA 2: ESTADO DE ESPACIOS =================
        var wsEspacios = workbook.Worksheets.Add("Estado de Espacios");

        string[] headersEspacios =
        {
            "ID Espacio",
            "Nombre del Espacio",
            "Tipo Espacio",
            "Campus",
            "Horario Operativo",
            "Capacidad Máxima",
            "Estado Actual",
            "Eventos en el Periodo"
        };

        FormatearEncabezado(wsEspacios, headersEspacios);

        int filaEsp = 2;
        foreach (var esp in listaEspacios)
        {
            int eventosDelEspacio = listaEventos.Count(e => e.IdEspacio == esp.Id);

            wsEspacios.Cell(filaEsp, 1).Value = esp.Id;
            wsEspacios.Cell(filaEsp, 2).Value = esp.Nombre;
            wsEspacios.Cell(filaEsp, 3).Value = esp.TipoEspacio?.Nombre ?? "N/A";
            wsEspacios.Cell(filaEsp, 4).Value = esp.Campus?.Nombre ?? $"Campus #{esp.IdCampus}";
            wsEspacios.Cell(filaEsp, 5).Value = $"{esp.HoraApertura:HH:mm} - {esp.HoraCierre:HH:mm}";
            wsEspacios.Cell(filaEsp, 6).Value = esp.CapacidadMaxima;
            wsEspacios.Cell(filaEsp, 7).Value = esp.Estado.ToString();
            wsEspacios.Cell(filaEsp, 8).Value = eventosDelEspacio;
            filaEsp++;
        }

        wsEspacios.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] GenerarReporteAsistencia(
        IEnumerable<Asistencia> asistencias,
        DateOnly fechaInicio,
        DateOnly fechaFin)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Control de Asistencia");

        string[] headers =
        {
            "Fecha Evento",
            "Campus",
            "Espacio",
            "Descripción del Evento",
            "Responsable del Evento",
            "Profesor / Usuario",
            "ID Institucional",
            "Hora Programada Inicio",
            "Hora Programada Fin",
            "Hora Entrada (Check-In)",
            "Hora Salida (Check-Out)",
            "Estado Asistencia",
            "Minutos de Retardo",
            "Tolerancia Aplicada (min)",
            "Observaciones"
        };

        FormatearEncabezado(ws, headers);

        int fila = 2;
        foreach (var a in asistencias)
        {
            ws.Cell(fila, 1).Value = a.FechaEvento.ToString("yyyy-MM-dd");
            ws.Cell(fila, 2).Value = a.Reserva?.Campus?.Nombre ?? "N/A";
            ws.Cell(fila, 3).Value = a.Reserva?.Espacio?.Nombre ?? "N/A";
            ws.Cell(fila, 4).Value = a.Reserva?.Descripcion ?? string.Empty;
            ws.Cell(fila, 5).Value = a.Reserva?.ResponsableEvento ?? string.Empty;
            ws.Cell(fila, 6).Value = a.Usuario?.NombreCompleto ?? a.IdUsuario;
            ws.Cell(fila, 7).Value = a.Usuario?.IdInstitucional ?? string.Empty;
            ws.Cell(fila, 8).Value = a.HoraProgramadaInicio.ToString("HH:mm");
            ws.Cell(fila, 9).Value = a.HoraProgramadaFin.ToString("HH:mm");
            ws.Cell(fila, 10).Value = a.HoraEntrada?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Sin registro";
            ws.Cell(fila, 11).Value = a.HoraSalida?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Sin registro";
            ws.Cell(fila, 12).Value = a.Estado.ToString();
            ws.Cell(fila, 13).Value = a.MinutosRetardo;
            ws.Cell(fila, 14).Value = a.MinutosToleranciaAplicados;
            ws.Cell(fila, 15).Value = a.Observaciones ?? string.Empty;
            fila++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static void FormatearEncabezado(IXLWorksheet worksheet, string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#004B87"); // Azul Institucional ITSON
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
    }
}
