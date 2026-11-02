using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ConfirmarOrcamentoGeradoCommandHandler(
    OrdemDeServicoHistoricoService historicoService,
    IOrdemDeServicoRepository ordemRepository)
    : IRequestHandler<ConfirmarOrcamentoGeradoCommand, Unit>
{
    public async Task<Unit> Handle(
        ConfirmarOrcamentoGeradoCommand request,
        CancellationToken cancellationToken)
    {
        var ordem = await ordemRepository.ObterPorIdAsync(request.OrdemServicoId, cancellationToken)
            ?? throw new ServiceNotFoundException(
                $"Ordem de servico com ID {request.OrdemServicoId} nao encontrada.");

        if (ordem.Status == StatusOrdemDeServico.AguardandoAprovacao)
            return Unit.Value;

        var evento = ordem.ConfirmarOrcamentoGerado(
            request.OrcamentoId,
            request.ValorTotal,
            request.OrcamentoGeradoEm);
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
