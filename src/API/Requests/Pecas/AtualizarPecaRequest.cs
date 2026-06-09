namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.Pecas;

public class AtualizarPecaRequest
{
    public string Nome { get; set; } = null!;
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
}
