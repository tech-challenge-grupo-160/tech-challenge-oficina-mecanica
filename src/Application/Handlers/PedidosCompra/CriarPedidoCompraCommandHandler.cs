using Fiap.TechChallenge.OficinaMecanica.Application.Commands.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.PedidosCompra;

public sealed class CriarPedidoCompraCommandHandler : IRequestHandler<CriarPedidoCompraCommand, PedidoCompraDto>
{
    private const string LoggerName = nameof(CriarPedidoCompraCommandHandler);
    private readonly IPedidoCompraRepository _pedidoCompraRepository;
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IOrdemServicoHistoricoRepository _historicoRepository;
    private readonly IUsuarioAutenticadoService _usuarioAutenticadoService;
    private readonly ITransactionManager _transactionManager;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public CriarPedidoCompraCommandHandler(
        IPedidoCompraRepository pedidoCompraRepository,
        IOrdemDeServicoRepository ordemDeServicoRepository,
        IPecaRepository pecaRepository,
        IOrdemServicoHistoricoRepository historicoRepository,
        IUsuarioAutenticadoService usuarioAutenticadoService,
        ITransactionManager transactionManager,
        IClock clock,
        ILoggerFactory loggerFactory)
    {
        _pedidoCompraRepository = pedidoCompraRepository;
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _pecaRepository = pecaRepository;
        _historicoRepository = historicoRepository;
        _usuarioAutenticadoService = usuarioAutenticadoService;
        _transactionManager = transactionManager;
        _clock = clock;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<PedidoCompraDto> Handle(CriarPedidoCompraCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            return await _transactionManager.ExecuteAsync(
                async token =>
                {
                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Criando pedido de compra manual");
                    var ordem = await _ordemDeServicoRepository.ObterPorIdAsync(command.OrdemDeServicoId, token);
                    if (ordem == null)
                    {
                        _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Ordem de servico nao encontrada para criacao manual de pedido");
                        throw new KeyNotFoundException($"Ordem de servico com ID {command.OrdemDeServicoId} nao encontrada.");
                    }

                    var peca = await _pecaRepository.ObterPorIdAsync(command.PecaId, token);
                    if (peca == null)
                    {
                        _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Peca nao encontrada para criacao manual de pedido");
                        throw new KeyNotFoundException($"Peca com ID {command.PecaId} nao encontrada.");
                    }

                    var pedido = await _pedidoCompraRepository.CriarAsync(
                        PedidoCompra.Criar(
                            command.OrdemDeServicoId,
                            command.PecaId,
                            command.QuantidadeSolicitada,
                            _clock.Now,
                            string.IsNullOrWhiteSpace(command.Observacao)
                                ? $"Pedido criado manualmente para a ordem {ordem.Numero}."
                                : command.Observacao),
                        token);

                    pedido.VincularPeca(peca);

                    await RegistrarHistoricoAsync(
                        ordem.Id,
                        TipoEventoOrdemServico.PedidoCompraGerado,
                        ordem.Status,
                        ordem.Status,
                        $"Pedido de compra {pedido.Id} criado manualmente para a peca {peca.Nome}. Quantidade solicitada: {command.QuantidadeSolicitada}.",
                        token);

                    _logger.LogInformation(LogTemplate.End, LoggerName, $"Pedido de compra {pedido.Id} criado manualmente com sucesso.");
                    return pedido.ToDto();
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
