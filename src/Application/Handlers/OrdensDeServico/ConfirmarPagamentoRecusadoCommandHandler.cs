using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

/// <summary>
/// Registra no historico da OS a recusa do pagamento (F4-27). Nada e desfeito:
/// a OS segue Finalizada e sem pagamento, o que ja bloqueia a entrega.
/// </summary>
public sealed class ConfirmarPagamentoRecusadoCommandHandler(
    IOrdemDeServicoRepository ordemRepository,
    OrdemDeServicoHistoricoService historicoService)
    : IRequestHandler<ConfirmarPagamentoRecusadoCommand, Unit>
{
    public async Task<Unit> Handle(
        ConfirmarPagamentoRecusadoCommand request,
        CancellationToken cancellationToken)
    {
        var ordem = await ordemRepository.ObterPorIdAsync(request.OrdemServicoId, cancellationToken)
            ?? throw new ServiceNotFoundException(
                $"Ordem de servico com ID {request.OrdemServicoId} nao encontrada.");

        var evento = ordem.RegistrarPagamentoRecusado(request.Motivo, request.Tentativa);
        await historicoService.RegistrarAsync(
            ordem,
            evento.TipoEvento,
            evento.StatusAnterior,
            evento.StatusNovo,
            evento.Descricao,
            cancellationToken);
        return Unit.Value;
    }
}
