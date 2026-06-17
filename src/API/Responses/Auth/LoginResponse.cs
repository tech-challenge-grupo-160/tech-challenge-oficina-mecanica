namespace Fiap.TechChallenge.OficinaMecanica.API.Responses.Auth;

public sealed class LoginResponse
{
    public string Token { get; init; } = null!;
    public DateTime ExpiraEm { get; init; }
    public string NomeUsuario { get; init; } = null!;
    public string Role { get; init; } = null!;
}
