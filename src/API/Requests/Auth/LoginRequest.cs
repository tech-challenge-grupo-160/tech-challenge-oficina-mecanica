namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.Auth;

public sealed class LoginRequest
{
    public string Usuario { get; init; } = null!;
    public string Senha { get; init; } = null!;
}
