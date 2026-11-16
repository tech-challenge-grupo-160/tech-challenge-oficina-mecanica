using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ConfirmarOrcamentoRecusadoCommandHandler(
    IOrdemDeServicoRepository ordemRepository,
    OrdemDeServicoHistoricoService historicoService)
    : IRequestHandler<ConfirmarOrcamentoRecusadoCommand, Unit>
{
    public async Task<Unit> Handle(
        ConfirmarOrcamentoRecusadoCommand request,
        CancellationToken cancellationToken)
    {
        var ordem = await ordemRepository.ObterPorIdAsync(request.OrdemServicoId, cancellationToken)
            ?? throw new ServiceNotFoundException(
                $"Ordem de servico com ID {request.OrdemServicoId} nao encontrada.");
        if (ordem.Status == StatusOrdemDeServico.Cancelada)
            return Unit.Value;

        var evento = ordem.RecusarOrcamento(request.Motivo);
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
