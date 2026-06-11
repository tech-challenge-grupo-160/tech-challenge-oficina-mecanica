using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class LiberarExecucaoCommandHandler : OrdemDeServicoHandlerBase, IRequestHandler<LiberarExecucaoCommand, OrdemDeServicoDto>
{
    public LiberarExecucaoCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(LiberarExecucaoCommand command, CancellationToken cancellationToken)
    {
        return LiberarExecucaoAsync(command.Id, cancellationToken);
    }
}

