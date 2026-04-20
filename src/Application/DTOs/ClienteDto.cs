namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string CpfCnpj { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DataCadastro { get; set; }
}

public class CriarClienteDto
{
    public string Nome { get; set; } = null!;
    public string CpfCnpj { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Email { get; set; } = null!;
}

public class AtualizarClienteDto
{
    public string Nome { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Email { get; set; } = null!;
}
