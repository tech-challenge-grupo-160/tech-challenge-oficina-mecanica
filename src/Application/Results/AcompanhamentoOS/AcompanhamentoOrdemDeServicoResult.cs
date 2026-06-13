namespace Fiap.TechChallenge.OficinaMecanica.Application.Results.AcompanhamentoOS;

public sealed class AcompanhamentoOrdemDeServicoResult
{
    public string Numero { get; init; } = null!;
    public string CodigoAcompanhamento { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime DataAbertura { get; init; }
    public DateTime DataUltimaAtualizacao { get; init; }
    public DateTime? OrcamentoEnviadoEm { get; init; }
    public DateTime? DataFinalizacao { get; init; }
    public DateTime? DataPagamento { get; init; }
    public DateTime? DataConclusao { get; init; }
}
