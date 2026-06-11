using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class AprovarOrdemDeServicoCommandHandler : OrdemDeServicoHandlerBase, IRequestHandler<AprovarOrdemDeServicoCommand, OrdemDeServicoDto>
{
    public AprovarOrdemDeServicoCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(AprovarOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        return AprovarAsync(command.Id, cancellationToken);
    }
}

