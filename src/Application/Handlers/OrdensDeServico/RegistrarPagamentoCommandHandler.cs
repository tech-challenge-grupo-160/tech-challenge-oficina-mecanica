using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class RegistrarPagamentoCommandHandler : OrdemDeServicoHandlerBase, IRequestHandler<RegistrarPagamentoCommand, OrdemDeServicoDto>
{
    public RegistrarPagamentoCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(RegistrarPagamentoCommand command, CancellationToken cancellationToken)
    {
        return RegistrarPagamentoAsync(command.Id, cancellationToken);
    }
}

