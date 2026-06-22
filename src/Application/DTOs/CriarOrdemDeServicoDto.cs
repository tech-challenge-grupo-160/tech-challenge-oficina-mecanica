namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class CriarOrdemDeServicoDto
{
    public int ClienteId { get; set; }
    public int VeiculoId { get; set; }
    public string DescricaoSolicitacao { get; set; } = null!;
    public string? ObservacoesRecepcao { get; set; }
    public IReadOnlyCollection<CriarOrdemDeServicoServicoDto> Servicos { get; set; } = [];
    public IReadOnlyCollection<CriarOrdemDeServicoPecaDto> Pecas { get; set; } = [];
}

public class CriarOrdemDeServicoServicoDto
{
    public int ServicoId { get; set; }
}

public class CriarOrdemDeServicoPecaDto
{
    public int PecaId { get; set; }
    public int Quantidade { get; set; }
}
