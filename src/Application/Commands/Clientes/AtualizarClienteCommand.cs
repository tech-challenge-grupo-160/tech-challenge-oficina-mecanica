namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;

public sealed class AtualizarClienteCommand
{
    public string Nome { get; init; } = null!;
    public string Telefone { get; init; } = null!;
    public string Email { get; init; } = null!;
}
