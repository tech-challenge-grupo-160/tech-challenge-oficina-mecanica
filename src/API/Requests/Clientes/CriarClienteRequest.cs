namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.Clientes;

public sealed class CriarClienteRequest
{
    public string Nome { get; set; } = null!;
    public string CpfCnpj { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Email { get; set; } = null!;
}
