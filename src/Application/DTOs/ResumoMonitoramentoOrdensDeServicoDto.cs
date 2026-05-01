namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class ResumoMonitoramentoOrdensDeServicoDto
{
    public int TotalOrdens { get; set; }
    public int TotalOrdensAbertas { get; set; }
    public int TotalOrdensFinalizadas { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int? TempoMedioFinalizacaoMinutos { get; set; }
    public double? TempoMedioFinalizacaoHoras { get; set; }
    public List<MonitoramentoOrdemDeServicoDto> Ordens { get; set; } = new();
}
