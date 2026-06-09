namespace Fiap.TechChallenge.OficinaMecanica.Application.Results.Servicos;

public class ServicoResult
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public decimal Preco { get; set; }
    public int TempoEstimado { get; set; }
}
