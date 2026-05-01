namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class CriarOrdemDeServicoDto
{
    public int ClienteId { get; set; }
    public int VeiculoId { get; set; }
    public string DescricaoSolicitacao { get; set; } = null!;
    public string? ObservacoesRecepcao { get; set; }
}
