namespace Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;

public sealed class OrdemDeServicoResult
{
    public int Id { get; init; }
    public string Numero { get; init; } = null!;
    public string CodigoAcompanhamento { get; init; } = null!;
    public string UrlAcompanhamento { get; init; } = null!;
    public string? TokenAcompanhamento { get; set; }
    public int ClienteId { get; init; }
    public int VeiculoId { get; init; }
    public string DescricaoSolicitacao { get; init; } = null!;
    public string? ObservacoesRecepcao { get; init; }
    public string? MotivoCancelamento { get; init; }
    public DateTime? OrcamentoEnviadoEm { get; init; }
    public DateTime? DataFinalizacao { get; init; }
    public DateTime? DataPagamento { get; init; }
    public string Status { get; init; } = null!;
    public DateTime DataAbertura { get; init; }
    public DateTime? DataConclusao { get; init; }
    public decimal ValorTotal { get; init; }
    public List<OrdemDeServicoServicoResult> Servicos { get; init; } = new();
    public List<OrdemDeServicoPecaResult> Pecas { get; init; } = new();
}
