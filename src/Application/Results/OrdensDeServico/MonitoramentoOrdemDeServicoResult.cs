namespace Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;

public sealed class MonitoramentoOrdemDeServicoResult
{
    public int Id { get; init; }
    public string Numero { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime DataAbertura { get; init; }
    public DateTime? DataFinalizacao { get; init; }
    public bool EstaFinalizada { get; init; }
    public int TempoDecorridoMinutos { get; init; }
    public double TempoDecorridoHoras { get; init; }
    public int? TempoFinalizacaoMinutos { get; init; }
    public double? TempoFinalizacaoHoras { get; init; }
}
