namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class AcompanhamentoOrdemDeServicoDto
{
    public string Numero { get; set; } = null!;
    public string CodigoAcompanhamento { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime DataAbertura { get; set; }
    public DateTime DataUltimaAtualizacao { get; set; }
    public DateTime? OrcamentoEnviadoEm { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public DateTime? DataPagamento { get; set; }
    public DateTime? DataConclusao { get; set; }
}
