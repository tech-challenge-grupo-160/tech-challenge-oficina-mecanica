using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ConfirmarReservaPecasCommandHandler(
    IOrdemDeServicoRepository ordemRepository,
    OrdemDeServicoHistoricoService historicoService)
    : IRequestHandler<ConfirmarReservaPecasCommand, Unit>
{
    public async Task<Unit> Handle(
        ConfirmarReservaPecasCommand request,
        CancellationToken cancellationToken)
    {
        var ordem = await ordemRepository.ObterPorIdAsync(request.OrdemServicoId, cancellationToken)
            ?? throw new ServiceNotFoundException(
                $"Ordem de servico com ID {request.OrdemServicoId} nao encontrada.");
        if (ordem.Status is StatusOrdemDeServico.EmExecucao or StatusOrdemDeServico.AguardandoEstoque)
            return Unit.Value;
        if (ordem.Status != StatusOrdemDeServico.AguardandoAprovacao)
            throw new InvalidOperationException(
                $"Resultado da reserva nao e valido no status atual da OS: {ordem.Status}.");

        var evento = request.ReservaCompleta
            ? ordem.ConfirmarPecasReservadas()
            : ordem.ConfirmarEstoqueInsuficiente(
                request.DescricaoFaltas ?? "estoque insuficiente");
        var atualizada = await ordemRepository.AtualizarAsync(ordem, cancellationToken);
        await historicoService.RegistrarAsync(
            atualizada,
            evento.TipoEvento,
            evento.StatusAnterior,
            evento.StatusNovo,
            evento.Descricao,
            cancellationToken);
        return Unit.Value;
    }
}
