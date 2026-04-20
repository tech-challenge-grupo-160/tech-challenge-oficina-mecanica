namespace Fiap.TechChallenge.OficinaMecanica.Application.Security;

public interface IUsuarioAutenticadoService
{
    UsuarioAutenticadoInfo ObterUsuarioAtual();
}

public sealed class UsuarioAutenticadoInfo
{
    public string? UsuarioId { get; init; }
    public string? UsuarioNome { get; init; }
}
