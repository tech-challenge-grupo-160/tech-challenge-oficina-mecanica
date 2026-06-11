using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class FinalizarOrdemDeServicoCommandHandler : OrdemDeServicoHandlerBase, IRequestHandler<FinalizarOrdemDeServicoCommand, OrdemDeServicoDto>
{
    public FinalizarOrdemDeServicoCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(FinalizarOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        return FinalizarAsync(command.Id, cancellationToken);
    }
}

