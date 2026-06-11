using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class RemoverServicoDaOrdemCommandHandler : OrdemDeServicoHandlerBase, IRequestHandler<RemoverServicoDaOrdemCommand, OrdemDeServicoDto>
{
    public RemoverServicoDaOrdemCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(RemoverServicoDaOrdemCommand command, CancellationToken cancellationToken)
    {
        return RemoverServicoAsync(command.OrdemDeServicoId, command.ServicoId, cancellationToken);
    }
}

