namespace SAAIC.Infrastructure.Services;

/// <summary>
/// Configuración para el envío de correos mediante la API de Gmail (HTTPS puerto 443, evitando el bloqueo institucional de puertos SMTP).
/// </summary>
public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string ApplicationName { get; set; } = "SAAIC - Reservas Culturales ITSON";
    public string SenderName { get; set; } = "Sistema de Reservas Culturales ITSON";
    public string SenderEmail { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Client ID de OAuth 2.0 en Google Cloud Console.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Client Secret de OAuth 2.0 en Google Cloud Console.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Refresh Token de OAuth 2.0 autorizado con el scope https://www.googleapis.com/auth/gmail.send.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
