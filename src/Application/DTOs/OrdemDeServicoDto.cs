using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class OrdemDeServicoDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = null!;
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

public class CriarOrdemDeServicoDto
{
    public int ClienteId { get; set; }
    public int VeiculoId { get; set; }
    public string DescricaoSolicitacao { get; set; } = null!;
    public string? ObservacoesRecepcao { get; set; }
}

public class AtualizarStatusOrdemDeServicoDto
{
    public string NovoStatus { get; set; } = null!;
}

public class CancelarOrdemDeServicoDto
{
    public string MotivoCancelamento { get; set; } = null!;
}

public class OrdemDeServicoServicoDto
{
    public int ServicoId { get; set; }
    public decimal Preco { get; set; }
    public int TempoEstimado { get; set; }
}

public class OrdemDeServicoPecaDto
{
    public int PecaId { get; set; }
    public int Quantidade { get; set; }
    public decimal Preco { get; set; }
}

public class AdicionarServicoAOrdemDto
{
    public int ServicoId { get; set; }
}

public class AdicionarPecaAOrdemDto
{
    public int PecaId { get; set; }
    public int Quantidade { get; set; }
}
