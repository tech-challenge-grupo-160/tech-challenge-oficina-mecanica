using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class RemoverPecaDaOrdemCommandHandler : OrdemDeServicoHandlerBase, IRequestHandler<RemoverPecaDaOrdemCommand, OrdemDeServicoDto>
{
    public RemoverPecaDaOrdemCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(RemoverPecaDaOrdemCommand command, CancellationToken cancellationToken)
    {
        return RemoverPecaAsync(command.OrdemDeServicoId, command.PecaId, cancellationToken);
    }
}

