using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.abstractions;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorUsuarioAsync(string usuarioLogin, CancellationToken cancellationToken = default);
}
