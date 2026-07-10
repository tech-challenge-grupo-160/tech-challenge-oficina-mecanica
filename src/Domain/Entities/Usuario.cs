namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class Usuario
{
    private Usuario()
    {
    }

    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string UsuarioLogin { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;
    public string Role { get; private set; } = null!;

    public static Usuario Criar(string nome, string usuarioLogin, string senhaHash, string role)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome do usuario e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(usuarioLogin))
        {
            throw new ArgumentException("Login do usuario e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(senhaHash))
        {
            throw new ArgumentException("Senha do usuario e obrigatoria.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role do usuario e obrigatoria.");
        }

        return new Usuario
        {
            Nome = nome.Trim(),
            UsuarioLogin = usuarioLogin.Trim(),
            SenhaHash = senhaHash,
            Role = role.Trim()
        };
    }
}
