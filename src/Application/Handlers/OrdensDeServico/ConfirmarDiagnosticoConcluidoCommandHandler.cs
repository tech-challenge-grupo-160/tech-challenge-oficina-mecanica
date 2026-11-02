using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ConfirmarDiagnosticoConcluidoCommandHandler(
    OrdemDeServicoHistoricoService historicoService,
    IOrdemDeServicoRepository ordemRepository)
    : IRequestHandler<ConfirmarDiagnosticoConcluidoCommand, Unit>
{
    public async Task<Unit> Handle(
        ConfirmarDiagnosticoConcluidoCommand request,
        CancellationToken cancellationToken)
    {
        var ordem = await ordemRepository.ObterPorIdAsync(request.OrdemServicoId, cancellationToken)
            ?? throw new ServiceNotFoundException(
                $"Ordem de servico com ID {request.OrdemServicoId} nao encontrada.");

        if (ordem.Status == StatusOrdemDeServico.AguardandoOrcamento)
            return Unit.Value;

        var evento = ordem.ConfirmarDiagnosticoConcluido(
            request.Servicos.Select(item => new ServicoDiagnosticoSnapshot(
                item.ServicoId,
                item.Preco,
                item.TempoEstimado)),
            request.Pecas.Select(item => new PecaDiagnosticoSnapshot(
                item.PecaId,
                item.Quantidade,
                item.Preco)));
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
