using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.PedidosCompra;

public sealed class ReceberPedidoCompraCommandHandler : IRequestHandler<ReceberPedidoCompraCommand, PedidoCompraResult>
{
    private const string LoggerName = nameof(ReceberPedidoCompraCommandHandler);
    private readonly IPedidoCompraRepository _pedidoCompraRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;
    private readonly IOrdemServicoHistoricoRepository _historicoRepository;
    private readonly IUsuarioAutenticadoService _usuarioAutenticadoService;
    private readonly ITransactionManager _transactionManager;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public ReceberPedidoCompraCommandHandler(
        IPedidoCompraRepository pedidoCompraRepository,
        IPecaRepository pecaRepository,
        IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
        IOrdemServicoHistoricoRepository historicoRepository,
        IUsuarioAutenticadoService usuarioAutenticadoService,
        ITransactionManager transactionManager,
        IClock clock,
        ILoggerFactory loggerFactory)
    {
        _pedidoCompraRepository = pedidoCompraRepository;
        _pecaRepository = pecaRepository;
        _movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
        _historicoRepository = historicoRepository;
        _usuarioAutenticadoService = usuarioAutenticadoService;
        _transactionManager = transactionManager;
        _clock = clock;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<PedidoCompraResult> Handle(ReceberPedidoCompraCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            return await _transactionManager.ExecuteAsync(
                async token =>
                {
                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando pedido de compra para recebimento");
                    var pedido = await _pedidoCompraRepository.ObterPorIdAsync(command.PedidoCompraId, token);
                    if (pedido == null)
                    {
                        _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Pedido de compra nao encontrado para recebimento");
                        throw new KeyNotFoundException($"Pedido de compra com ID {command.PedidoCompraId} nao encontrado.");
                    }

                    var peca = await _pecaRepository.ObterPorIdAsync(pedido.PecaId, token);
                    if (peca == null)
                    {
                        throw new KeyNotFoundException($"Peca com ID {pedido.PecaId} nao encontrada para recebimento do pedido.");
                    }

                    var estoqueAnterior = peca.QuantidadeEstoque;
                    var agora = _clock.Now;
                    pedido.RegistrarRecebimento(command.QuantidadeRecebida, agora);
                    peca.ReporEstoque(command.QuantidadeRecebida);

                    await _pedidoCompraRepository.AtualizarAsync(pedido, token);
                    await _pecaRepository.AtualizarAsync(peca, token);
                    await _movimentacaoEstoqueRepository.CriarAsync(
                        MovimentacaoEstoque.Registrar(
                            peca.Id,
                            pedido.OrdemDeServicoId,
                            pedido.Id,
                            TipoMovimentacaoEstoque.EntradaPorPedidoCompra,
                            command.QuantidadeRecebida,
                            estoqueAnterior,
                            peca.QuantidadeEstoque,
                            $"Entrada de estoque por recebimento do pedido de compra {pedido.Id}.",
                            agora),
                        token);

                    await RegistrarHistoricoAsync(
                        pedido.OrdemDeServicoId,
                        TipoEventoOrdemServico.PedidoCompraRecebido,
                        null,
                        null,
                        $"Pedido de compra {pedido.Id} recebido para a peca {peca.Nome}. Quantidade recebida: {command.QuantidadeRecebida}.",
                        token);
                    await RegistrarHistoricoAsync(
                        pedido.OrdemDeServicoId,
                        TipoEventoOrdemServico.EstoqueReposto,
                        null,
                        null,
                        $"Estoque reposto para a peca {peca.Nome}. Saldo atual: {peca.QuantidadeEstoque}.",
                        token);

                    pedido.VincularPeca(peca);

                    _logger.LogInformation(LogTemplate.End, LoggerName, $"Recebimento do pedido de compra {pedido.Id} registrado com sucesso.");
                    return pedido.ToResult();
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    private async Task RegistrarHistoricoAsync(
        int ordemDeServicoId,
        TipoEventoOrdemServico tipoEvento,
        StatusOrdemDeServico? statusAnterior,
        StatusOrdemDeServico? statusNovo,
        string descricao,
        CancellationToken cancellationToken)
    {
        var usuarioAtual = _usuarioAutenticadoService.ObterUsuarioAtual();

        await _historicoRepository.CriarAsync(
            OrdemServicoHistorico.Registrar(
                ordemDeServicoId,
                usuarioAtual.UsuarioId,
                usuarioAtual.UsuarioNome,
                statusAnterior,
                statusNovo,
                tipoEvento,
                descricao,
                _clock.Now),
            cancellationToken);
    }
}
