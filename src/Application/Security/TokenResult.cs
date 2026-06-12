namespace Fiap.TechChallenge.OficinaMecanica.Application.Security;

public sealed class TokenResult
{
    public string Token { get; init; } = null!;
    public DateTime ExpiraEm { get; init; }
}
