namespace Fiap.TechChallenge.OficinaMecanica.API.Responses.Clientes;

public sealed class ClienteResponse
{
    public int Id { get; init; }
    public string Nome { get; init; } = null!;
    public string CpfCnpj { get; init; } = null!;
    public string Telefone { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime DataCadastro { get; init; }
}
