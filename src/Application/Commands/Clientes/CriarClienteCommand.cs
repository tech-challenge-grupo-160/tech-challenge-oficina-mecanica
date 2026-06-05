namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;

public sealed class CriarClienteCommand
{
    public string Nome { get; init; } = null!;
    public string CpfCnpj { get; init; } = null!;
    public string Telefone { get; init; } = null!;
    public string Email { get; init; } = null!;
}
