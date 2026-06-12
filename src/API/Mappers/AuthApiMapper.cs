using Fiap.TechChallenge.OficinaMecanica.API.Requests.Auth;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Auth;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Auth;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Auth;

namespace Fiap.TechChallenge.OficinaMecanica.API.Mappers;

public static class AuthApiMapper
{
    public static LoginCommand ToCommand(this LoginRequest request)
    {
        return new LoginCommand
        {
            Usuario = request.Usuario,
            Senha = request.Senha
        };
    }

    public static LoginResponse ToResponse(this LoginResult result)
    {
        return new LoginResponse
        {
            Token = result.Token,
            ExpiraEm = result.ExpiraEm,
            NomeUsuario = result.NomeUsuario,
            Role = result.Role
        };
    }
}
