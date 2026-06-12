using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Security;

public interface ITokenGenerator
{
    TokenResult Gerar(Usuario usuario);
}
