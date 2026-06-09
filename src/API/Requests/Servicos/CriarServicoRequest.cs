namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.Servicos;

public class CriarServicoRequest
{
    public string Nome { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public decimal Preco { get; set; }
    public int TempoEstimado { get; set; }
}
