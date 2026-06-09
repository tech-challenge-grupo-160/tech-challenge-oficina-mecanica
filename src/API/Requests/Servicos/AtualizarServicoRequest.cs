namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.Servicos;

public class AtualizarServicoRequest
{
    public string Nome { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public decimal Preco { get; set; }
    public int TempoEstimado { get; set; }
}
