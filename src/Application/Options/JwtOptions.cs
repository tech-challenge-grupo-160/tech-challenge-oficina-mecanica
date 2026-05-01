using System.Diagnostics.CodeAnalysis;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Options;

[ExcludeFromCodeCoverage]
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public int ExpirationMinutes { get; set; } = 60;
}
