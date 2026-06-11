using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed class CriarOrdemDeServicoCommand : IRequest<OrdemDeServicoDto>
{
    public int ClienteId { get; init; }
    public int VeiculoId { get; init; }
    public string DescricaoSolicitacao { get; init; } = null!;
    public string? ObservacoesRecepcao { get; init; }
}

