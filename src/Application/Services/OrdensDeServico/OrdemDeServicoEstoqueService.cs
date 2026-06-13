using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;

public sealed class OrdemDeServicoEstoqueService
{
    private readonly IPecaRepository _pecaRepository;
    private readonly IPedidoCompraRepository _pedidoCompraRepository;
    private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly IClock _clock;

    public OrdemDeServicoEstoqueService(
        IPecaRepository pecaRepository,
        IPedidoCompraRepository pedidoCompraRepository,
        IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
        OrdemDeServicoHistoricoService historicoService,
        IClock clock)
    {
        _pecaRepository = pecaRepository;
        _pedidoCompraRepository = pedidoCompraRepository;
        _movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
        _historicoService = historicoService;
        _clock = clock;
    }

    public async Task<List<FaltaEstoqueItem>> ObterFaltasAsync(OrdemDeServico ordem, CancellationToken cancellationToken)
    {
        var faltas = new List<FaltaEstoqueItem>();

        foreach (var item in ordem.Pecas)
        {
            var peca = await _pecaRepository.ObterPorIdAsync(item.PecaId, cancellationToken);
            if (peca == null)
            {
                throw new KeyNotFoundException($"Peca com ID {item.PecaId} nao encontrada para validacao de estoque.");
            }

            if (peca.QuantidadeEstoque < item.Quantidade)
            {
                faltas.Add(new FaltaEstoqueItem(peca, item.Quantidade - peca.QuantidadeEstoque));
            }
        }

        return faltas;
    }

    public async Task BaixarEstoqueDaOrdemAsync(OrdemDeServico ordem, CancellationToken cancellationToken)
    {
        if (!ordem.Pecas.Any())
        {
            return;
        }

        var movimentos = new List<string>();

        foreach (var item in ordem.Pecas)
        {
            var peca = await _pecaRepository.ObterPorIdAsync(item.PecaId, cancellationToken);
            if (peca == null)
            {
                throw new KeyNotFoundException($"Peca com ID {item.PecaId} nao encontrada para baixa de estoque.");
            }

            var estoqueAnterior = peca.QuantidadeEstoque;
            peca.BaixarEstoque(item.Quantidade);
            await _pecaRepository.AtualizarAsync(peca, cancellationToken);
            await _movimentacaoEstoqueRepository.CriarAsync(
                MovimentacaoEstoque.Registrar(
                    peca.Id,
                    ordem.Id,
                    null,
                    TipoMovimentacaoEstoque.BaixaParaOrdemDeServico,
                    item.Quantidade,
                    estoqueAnterior,
                    peca.QuantidadeEstoque,
                    $"Baixa de estoque para a ordem de servico {ordem.Numero}.",
                    _clock.Now),
                cancellationToken);
            movimentos.Add($"{peca.Nome} x{item.Quantidade}");
        }

        await _historicoService.RegistrarAsync(
            ordem,
            TipoEventoOrdemServico.EstoqueBaixado,
            ordem.Status,
            ordem.Status,
            $"Baixa de estoque registrada para a ordem: {string.Join(", ", movimentos)}.",
            cancellationToken);
    }

    public async Task CriarOuAtualizarPedidoCompraAsync(OrdemDeServico ordem, Peca peca, int quantidadeFaltante, CancellationToken cancellationToken)
    {
        var pedidoExistente = await _pedidoCompraRepository.ObterPendentePorOrdemEPecaAsync(ordem.Id, peca.Id, cancellationToken);
        if (pedidoExistente == null)
        {
            var pedidoCriado = await _pedidoCompraRepository.CriarAsync(
                PedidoCompra.Criar(
                    ordem.Id,
                    peca.Id,
                    quantidadeFaltante,
                    _clock.Now,
                    $"Pedido gerado automaticamente por falta de estoque para a ordem {ordem.Numero}."),
                cancellationToken);

            await _historicoService.RegistrarAsync(
                ordem,
                TipoEventoOrdemServico.PedidoCompraGerado,
                ordem.Status,
                ordem.Status,
                $"Pedido de compra {pedidoCriado.Id} gerado para a peca {peca.Nome}. Quantidade solicitada: {quantidadeFaltante}.",
                cancellationToken);
            return;
        }

        if (pedidoExistente.QuantidadeSolicitada < quantidadeFaltante)
        {
            pedidoExistente.AtualizarQuantidadeSolicitada(quantidadeFaltante);
            await _pedidoCompraRepository.AtualizarAsync(pedidoExistente, cancellationToken);
        }

        await _historicoService.RegistrarAsync(
            ordem,
            TipoEventoOrdemServico.PedidoCompraGerado,
            ordem.Status,
            ordem.Status,
            $"Pedido de compra pendente mantido para a peca {peca.Nome}. Quantidade solicitada: {pedidoExistente.QuantidadeSolicitada}.",
            cancellationToken);
    }

    public static string FormatarFaltas(IEnumerable<FaltaEstoqueItem> faltas)
    {
        return string.Join(", ", faltas.Select(x => $"{x.Peca.Nome} ({x.QuantidadeFaltante})"));
    }
}

public sealed record FaltaEstoqueItem(Peca Peca, int QuantidadeFaltante);
