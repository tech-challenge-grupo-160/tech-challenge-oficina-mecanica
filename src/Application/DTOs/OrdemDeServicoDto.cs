using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class OrdemDeServicoDto
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = null!;
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime DataAbertura { get; set; }
    public DateTime? DataConclusao { get; set; }
    public decimal ValorTotal { get; set; }
    public List<OrdemDeServicoServicoDto> Servicos { get; set; } = new();
    public List<OrdemDeServicoPecaDto> Pecas { get; set; } = new();
}

public class CriarOrdemDeServicoDto
{
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
}

public class AtualizarStatusOrdemDeServicoDto
{
    public string NovoStatus { get; set; } = null!;
}

public class OrdemDeServicoServicoDto
{
    public Guid ServicoId { get; set; }
    public decimal Preco { get; set; }
    public int TempoEstimado { get; set; }
}

public class OrdemDeServicoPecaDto
{
    public Guid PecaId { get; set; }
    public int Quantidade { get; set; }
    public decimal Preco { get; set; }
}

public class AdicionarServicoAOrdemDto
{
    public Guid ServicoId { get; set; }
}

public class AdicionarPecaAOrdemDto
{
    public Guid PecaId { get; set; }
    public int Quantidade { get; set; }
}
