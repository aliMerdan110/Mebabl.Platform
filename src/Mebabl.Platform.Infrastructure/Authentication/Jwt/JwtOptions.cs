namespace Mebabl.Platform.Infrastructure.Authentication.Jwt;

public sealed class JwtOptions
{
    public const string SectionName = "JwtSettings";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; }
}