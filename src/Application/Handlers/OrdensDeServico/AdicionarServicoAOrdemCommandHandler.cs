using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class AdicionarServicoAOrdemCommandHandler : OrdemDeServicoHandlerBase, IRequestHandler<AdicionarServicoAOrdemCommand, OrdemDeServicoDto>
{
    public AdicionarServicoAOrdemCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(AdicionarServicoAOrdemCommand command, CancellationToken cancellationToken)
    {
        return AdicionarServicoAsync(command.OrdemDeServicoId, new AdicionarServicoAOrdemDto
        {
            ServicoId = command.ServicoId
        }, cancellationToken);
    }
}

