using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class CriarOrdemDeServicoCommandHandler : OrdemDeServicoHandlerBase, IRequestHandler<CriarOrdemDeServicoCommand, OrdemDeServicoDto>
{
    public CriarOrdemDeServicoCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(CriarOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        return CriarOrdemDeServicoAsync(new CriarOrdemDeServicoDto
        {
            ClienteId = command.ClienteId,
            VeiculoId = command.VeiculoId,
            DescricaoSolicitacao = command.DescricaoSolicitacao,
            ObservacoesRecepcao = command.ObservacoesRecepcao
        }, cancellationToken);
    }
}

