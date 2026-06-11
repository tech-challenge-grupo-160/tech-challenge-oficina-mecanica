using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class FinalizarDiagnosticoCommandHandler : OrdemDeServicoHandlerBase, IRequestHandler<FinalizarDiagnosticoCommand, OrdemDeServicoDto>
{
    public FinalizarDiagnosticoCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(FinalizarDiagnosticoCommand command, CancellationToken cancellationToken)
    {
        return FinalizarDiagnosticoAsync(command.Id, cancellationToken);
    }
}

