namespace Fiap.TechChallenge.OficinaMecanica.Application.Results.Auth;

public sealed class LoginResult
{
    public string Token { get; init; } = null!;
    public DateTime ExpiraEm { get; init; }
    public string NomeUsuario { get; init; } = null!;
    public string Role { get; init; } = null!;
}
