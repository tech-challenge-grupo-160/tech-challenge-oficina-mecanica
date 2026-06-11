using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class AdicionarPecaAOrdemCommandHandler : OrdemDeServicoHandlerBase, IRequestHandler<AdicionarPecaAOrdemCommand, OrdemDeServicoDto>
{
    public AdicionarPecaAOrdemCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(AdicionarPecaAOrdemCommand command, CancellationToken cancellationToken)
    {
        return AdicionarPecaAsync(command.OrdemDeServicoId, new AdicionarPecaAOrdemDto
        {
            PecaId = command.PecaId,
            Quantidade = command.Quantidade
        }, cancellationToken);
    }
}

