using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ConfirmarOrcamentoAprovadoCommandHandler(
    IOrdemDeServicoRepository ordemRepository,
    OrdemDeServicoHistoricoService historicoService)
    : IRequestHandler<ConfirmarOrcamentoAprovadoCommand, SolicitacaoReservaPecasResult>
{
    public async Task<SolicitacaoReservaPecasResult> Handle(
        ConfirmarOrcamentoAprovadoCommand request,
        CancellationToken cancellationToken)
    {
        var ordem = await ordemRepository.ObterPorIdAsync(request.OrdemServicoId, cancellationToken)
            ?? throw new ServiceNotFoundException(
                $"Ordem de servico com ID {request.OrdemServicoId} nao encontrada.");

        if (ordem.Status is StatusOrdemDeServico.EmExecucao or StatusOrdemDeServico.AguardandoEstoque)
            return new SolicitacaoReservaPecasResult(false, []);
        if (ordem.Status != StatusOrdemDeServico.AguardandoAprovacao)
            throw new InvalidOperationException(
                $"Orcamento nao pode ser aprovado no status atual da OS: {ordem.Status}.");
        if (ordem.ValorTotal != request.ValorTotal)
            throw new InvalidOperationException("Valor aprovado difere do total atual da ordem de servico.");

        if (ordem.Pecas.Count == 0)
        {
            var evento = ordem.LiberarExecucaoComEvento();
            var atualizada = await ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await historicoService.RegistrarAsync(
                atualizada,
                evento.TipoEvento,
                evento.StatusAnterior,
                evento.StatusNovo,
                evento.Descricao,
                cancellationToken);
            return new SolicitacaoReservaPecasResult(false, []);
        }

        var eventoReserva = ordem.RegistrarAprovacaoAguardandoReserva();
        await historicoService.RegistrarAsync(
            ordem,
            eventoReserva.TipoEvento,
            eventoReserva.StatusAnterior,
            eventoReserva.StatusNovo,
            eventoReserva.Descricao,
            cancellationToken);
        return new SolicitacaoReservaPecasResult(
            true,
            ordem.Pecas.Select(item => new OrdemDeServicoPecaResult
            {
                PecaId = item.PecaId,
                Quantidade = item.Quantidade,
                Preco = item.Preco
            }).ToArray());
    }
}
