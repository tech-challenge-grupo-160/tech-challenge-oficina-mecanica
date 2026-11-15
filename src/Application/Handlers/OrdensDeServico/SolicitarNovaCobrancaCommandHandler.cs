using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

/// <summary>
/// Nova cobranca depois de um pagamento recusado (F4-27). A OS precisa estar
/// Finalizada e sem pagamento; a saga precisa estar em PagamentoRecusado. O
/// Billing cria um pagamento novo para a tentativa, sem duplicar cobranca.
/// </summary>
public sealed class SolicitarNovaCobrancaCommandHandler(
    IOrdemDeServicoRepository ordemRepository,
    ISagaPagamentoService sagaPagamento,
    OrdemDeServicoHistoricoService historicoService)
    : IRequestHandler<SolicitarNovaCobrancaCommand, OrdemDeServicoResult>
{
    public async Task<OrdemDeServicoResult> Handle(
        SolicitarNovaCobrancaCommand request,
        CancellationToken cancellationToken)
    {
        var ordem = await ordemRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new ServiceNotFoundException($"Ordem de servico com ID {request.Id} nao encontrada.");
        ordem.ValidarNovaCobranca();

        var tentativa = await sagaPagamento.SolicitarNovaTentativaAsync(request.Id, cancellationToken);
        var evento = ordem.SolicitarNovaCobranca(tentativa);
        await historicoService.RegistrarAsync(
            ordem,
            evento.TipoEvento,
            evento.StatusAnterior,
            evento.StatusNovo,
            evento.Descricao,
            cancellationToken);
        return OrdemDeServicoMapper.ToResult(ordem);
    }
}
