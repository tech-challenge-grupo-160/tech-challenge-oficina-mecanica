namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.Clientes;

public sealed class AtualizarClienteRequest
{
    public string Nome { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Email { get; set; } = null!;
}
