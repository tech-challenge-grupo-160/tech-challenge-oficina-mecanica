using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed record ConfirmarOrcamentoAprovadoCommand(
    int OrdemServicoId,
    decimal ValorTotal)
    : IRequest<SolicitacaoReservaPecasResult>;

public sealed record SolicitacaoReservaPecasResult(
    bool ReservaNecessaria,
    IReadOnlyCollection<OrdemDeServicoPecaResult> Pecas);
