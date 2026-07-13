namespace Fiap.TechChallenge.OficinaMecanica.API.Responses.Pecas;

public class PecaResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
}
