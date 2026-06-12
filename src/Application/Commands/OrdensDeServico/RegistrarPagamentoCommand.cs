using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed class RegistrarPagamentoCommand : IRequest<OrdemDeServicoResult>
{
    public int Id { get; init; }
}

