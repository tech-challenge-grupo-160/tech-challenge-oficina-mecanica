using System.Text.Json.Serialization;

namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class OrdemDeServicoDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = null!;
    public string CodigoAcompanhamento { get; set; } = null!;
    public string UrlAcompanhamento { get; set; } = null!;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TokenAcompanhamento { get; set; }
    public int ClienteId { get; set; }
    public int VeiculoId { get; set; }
    public string DescricaoSolicitacao { get; set; } = null!;
    public string? ObservacoesRecepcao { get; set; }
    public string? MotivoCancelamento { get; set; }
    public DateTime? OrcamentoEnviadoEm { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public DateTime? DataPagamento { get; set; }
    public string Status { get; set; } = null!;
    public DateTime DataAbertura { get; set; }
    public DateTime? DataConclusao { get; set; }
    public decimal ValorTotal { get; set; }
    public List<OrdemDeServicoServicoDto> Servicos { get; set; } = new();
    public List<OrdemDeServicoPecaDto> Pecas { get; set; } = new();
}
