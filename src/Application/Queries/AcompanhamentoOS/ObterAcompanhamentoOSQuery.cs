using Fiap.TechChallenge.OficinaMecanica.Application.Results.AcompanhamentoOS;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.AcompanhamentoOS;

public sealed class ObterAcompanhamentoOSQuery : IRequest<AcompanhamentoOrdemDeServicoResult>
{
    public string Codigo { get; init; } = null!;
    public string Token { get; init; } = null!;
}
