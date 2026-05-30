namespace Frametric.Infrastructure.Security;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; }
}
