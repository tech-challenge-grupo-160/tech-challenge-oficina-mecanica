namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string UsuarioLogin { get; set; } = null!;
    public string SenhaHash { get; set; } = null!;
    public string Role { get; set; } = null!;
}
