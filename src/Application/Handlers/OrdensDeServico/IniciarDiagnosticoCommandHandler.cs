using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class IniciarDiagnosticoCommandHandler : OrdemDeServicoHandlerBase, IRequestHandler<IniciarDiagnosticoCommand, OrdemDeServicoDto>
{
    public IniciarDiagnosticoCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(IniciarDiagnosticoCommand command, CancellationToken cancellationToken)
    {
        return IniciarDiagnosticoAsync(command.Id, cancellationToken);
    }
}

