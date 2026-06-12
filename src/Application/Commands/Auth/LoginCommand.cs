using Fiap.TechChallenge.OficinaMecanica.Application.Results.Auth;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.Auth;

public sealed class LoginCommand : IRequest<LoginResult>
{
    public string Usuario { get; init; } = null!;
    public string Senha { get; init; } = null!;
}
