namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class PecaDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
}

public class CriarPecaDto
{
    public string Nome { get; set; } = null!;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
}

public class AtualizarPecaDto
{
    public string Nome { get; set; } = null!;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
}
