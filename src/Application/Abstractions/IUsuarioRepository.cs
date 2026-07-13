using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorUsuarioAsync(string usuarioLogin, CancellationToken cancellationToken = default);
}
