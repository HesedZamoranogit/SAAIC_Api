namespace SAAIC.Infrastructure.Services;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SAAIC.ITSON.Api";
    public string Audience { get; set; } = "SAAIC.ITSON.IntranetClient";
    public int ExpirationMinutes { get; set; } = 480; // 8 horas jornada laboral
}
