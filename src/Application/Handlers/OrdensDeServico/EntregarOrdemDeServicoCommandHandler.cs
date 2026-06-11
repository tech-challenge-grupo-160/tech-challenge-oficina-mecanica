using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class EntregarOrdemDeServicoCommandHandler : OrdemDeServicoHandlerBase, IRequestHandler<EntregarOrdemDeServicoCommand, OrdemDeServicoDto>
{
    public EntregarOrdemDeServicoCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(EntregarOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        return EntregarAsync(command.Id, cancellationToken);
    }
}

