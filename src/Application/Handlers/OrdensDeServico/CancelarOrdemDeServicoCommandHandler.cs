using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class CancelarOrdemDeServicoCommandHandler : OrdemDeServicoHandlerBase, IRequestHandler<CancelarOrdemDeServicoCommand, OrdemDeServicoDto>
{
    public CancelarOrdemDeServicoCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(CancelarOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        return CancelarAsync(command.Id, new CancelarOrdemDeServicoDto
        {
            MotivoCancelamento = command.MotivoCancelamento
        }, cancellationToken);
    }
}

