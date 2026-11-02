namespace SAAIC.Application.Interfaces.Services;

public interface IEmailService
{
    /// <summary>
    /// Envía un correo electrónico en formato HTML (usado para recuperación de contraseña con token, notificaciones, etc.).
    /// </summary>
    Task SendEmailAsync(
        string destinatario,
        string asunto,
        string cuerpoHtml,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía un correo electrónico con un archivo adjunto (usado para el envío automático de los reportes diarios en Excel).
    /// </summary>
    Task SendEmailWithAttachmentAsync(
        IEnumerable<string> destinatarios,
        string asunto,
        string cuerpoHtml,
        byte[] archivoAdjuntoBytes,
        string nombreArchivoAdjunto,
        string contentTypeAdjunto = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        CancellationToken cancellationToken = default);
}
