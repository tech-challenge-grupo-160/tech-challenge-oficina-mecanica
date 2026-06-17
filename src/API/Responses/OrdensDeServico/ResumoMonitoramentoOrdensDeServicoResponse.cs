namespace Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;

public sealed class ResumoMonitoramentoOrdensDeServicoResponse
{
    public int TotalOrdens { get; init; }
    public int TotalOrdensAbertas { get; init; }
    public int TotalOrdensFinalizadas { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public int? TempoMedioFinalizacaoMinutos { get; init; }
    public double? TempoMedioFinalizacaoHoras { get; init; }
    public List<MonitoramentoOrdemDeServicoResponse> Ordens { get; init; } = new();
}
