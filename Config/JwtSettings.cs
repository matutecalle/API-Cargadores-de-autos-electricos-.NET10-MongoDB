namespace APICargadores.Config;

/// <summary>
/// Parámetros para emitir y validar tokens JWT. Se enlaza desde la sección "Jwt".
/// </summary>
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "APICargadores";
    public string Audience { get; set; } = "APICargadores";
    public int ExpiryMinutes { get; set; } = 120;
}
