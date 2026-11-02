using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SAAIC.Application.Interfaces.Services;

namespace SAAIC.Infrastructure.Services;

/// <summary>
/// Servicio de correo basado en la API REST de Gmail (HTTPS / Puerto 443)
/// para evitar bloqueos de firewall institucional sobre puertos SMTP (25/465/587).
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailOptions, ILogger<EmailService> logger)
    {
        _emailSettings = emailOptions.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string destinatario,
        string asunto,
        string cuerpoHtml,
        CancellationToken cancellationToken = default)
    {
        if (!CredencialesGmailConfiguradas())
        {
            _logger.LogWarning(
                "[EmailService - Gmail API no configurada] Para: {Destinatario} | Asunto: {Asunto} | Contenido: {Cuerpo}",
                destinatario, asunto, cuerpoHtml);
            return;
        }

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        mimeMessage.To.Add(MailboxAddress.Parse(destinatario.Trim()));
        mimeMessage.Subject = asunto;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = cuerpoHtml
        };
        mimeMessage.Body = bodyBuilder.ToMessageBody();

        await EnviarPorGmailApiAsync(mimeMessage, cancellationToken);
    }

    public async Task SendEmailWithAttachmentAsync(
        IEnumerable<string> destinatarios,
        string asunto,
        string cuerpoHtml,
        byte[] archivoAdjuntoBytes,
        string nombreArchivoAdjunto,
        string contentTypeAdjunto = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        CancellationToken cancellationToken = default)
    {
        var listaCorreos = destinatarios
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d.Trim())
            .Distinct()
            .ToList();

        if (listaCorreos.Count == 0)
            return;

        if (!CredencialesGmailConfiguradas())
        {
            _logger.LogWarning(
                "[EmailService - Gmail API no configurada] Reporte adjunto '{Archivo}' ({Bytes} bytes) simulado hacia: {Destinatarios}",
                nombreArchivoAdjunto, archivoAdjuntoBytes.Length, string.Join(", ", listaCorreos));
            return;
        }

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));

        foreach (var correo in listaCorreos)
        {
            mimeMessage.To.Add(MailboxAddress.Parse(correo));
        }

        mimeMessage.Subject = asunto;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = cuerpoHtml
        };

        bodyBuilder.Attachments.Add(
            nombreArchivoAdjunto,
            archivoAdjuntoBytes,
            ContentType.Parse(contentTypeAdjunto));

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        await EnviarPorGmailApiAsync(mimeMessage, cancellationToken);
    }

    private async Task EnviarPorGmailApiAsync(MimeMessage mimeMessage, CancellationToken cancellationToken)
    {
        using var gmailService = CrearGmailService();

        using var memoryStream = new MemoryStream();
        await mimeMessage.WriteToAsync(memoryStream, cancellationToken);

        var rawBase64Url = Convert.ToBase64String(memoryStream.ToArray())
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var gmailMessage = new Message
        {
            Raw = rawBase64Url
        };

        var request = gmailService.Users.Messages.Send(gmailMessage, "me");
        await request.ExecuteAsync(cancellationToken);
    }

    private GmailService CrearGmailService()
    {
        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _emailSettings.ClientId,
                ClientSecret = _emailSettings.ClientSecret
            },
            Scopes = new[] { GmailService.Scope.GmailSend }
        });

        var tokenResponse = new TokenResponse
        {
            RefreshToken = _emailSettings.RefreshToken
        };

        var credential = new UserCredential(flow, "me", tokenResponse);

        return new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _emailSettings.ApplicationName
        });
    }

    private bool CredencialesGmailConfiguradas()
    {
        return !string.IsNullOrWhiteSpace(_emailSettings.SenderEmail) &&
               !string.IsNullOrWhiteSpace(_emailSettings.ClientId) &&
               !string.IsNullOrWhiteSpace(_emailSettings.ClientSecret) &&
               !string.IsNullOrWhiteSpace(_emailSettings.RefreshToken);
    }
}
