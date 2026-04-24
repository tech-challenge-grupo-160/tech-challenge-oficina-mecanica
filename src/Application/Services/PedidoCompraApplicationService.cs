using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface IPedidoCompraApplicationService
{
    Task<PedidoCompraDto> CriarAsync(CriarPedidoCompraDto dto, CancellationToken cancellationToken);
    Task<PagedResultDto<PedidoCompraDto>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<IEnumerable<PedidoCompraDto>> ListarPorOrdemDeServicoAsync(int ordemDeServicoId, CancellationToken cancellationToken);
    Task<PedidoCompraDto> RegistrarRecebimentoAsync(int pedidoCompraId, ReceberPedidoCompraDto dto, CancellationToken cancellationToken);
}

public class PedidoCompraApplicationService : IPedidoCompraApplicationService
{
    private const string LoggerName = nameof(PedidoCompraApplicationService);
    private readonly IPedidoCompraRepository _pedidoCompraRepository;
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;
    private readonly IOrdemServicoHistoricoRepository _historicoRepository;
    private readonly IUsuarioAutenticadoService _usuarioAutenticadoService;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger _logger;

    public PedidoCompraApplicationService(
        IPedidoCompraRepository pedidoCompraRepository,
        IOrdemDeServicoRepository ordemDeServicoRepository,
        IPecaRepository pecaRepository,
        IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
        IOrdemServicoHistoricoRepository historicoRepository,
        IUsuarioAutenticadoService usuarioAutenticadoService,
        ITransactionManager transactionManager,
        ILoggerFactory loggerFactory)
    {
        _pedidoCompraRepository = pedidoCompraRepository;
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _pecaRepository = pecaRepository;
        _movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
        _historicoRepository = historicoRepository;
        _usuarioAutenticadoService = usuarioAutenticadoService;
        _transactionManager = transactionManager;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<PedidoCompraDto> CriarAsync(CriarPedidoCompraDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            return await _transactionManager.ExecuteAsync(
                async token =>
                {
                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarAsync), "Criando pedido de compra manual");
                    var ordem = await _ordemDeServicoRepository.ObterPorIdAsync(dto.OrdemDeServicoId, token);
                    if (ordem == null)
                    {
                        _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarAsync), "Ordem de servico nao encontrada para criacao manual de pedido");
                        throw new KeyNotFoundException($"Ordem de servico com ID {dto.OrdemDeServicoId} nao encontrada.");
                    }

                    var peca = await _pecaRepository.ObterPorIdAsync(dto.PecaId, token);
                    if (peca == null)
                    {
                        _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarAsync), "Peca nao encontrada para criacao manual de pedido");
                        throw new KeyNotFoundException($"Peca com ID {dto.PecaId} nao encontrada.");
                    }

                    if (dto.QuantidadeSolicitada <= 0)
                    {
                        throw new InvalidOperationException("A quantidade solicitada deve ser maior que zero.");
                    }

                    var pedido = await _pedidoCompraRepository.CriarAsync(
                        new PedidoCompra
                        {
                            OrdemDeServicoId = dto.OrdemDeServicoId,
                            PecaId = dto.PecaId,
                            QuantidadeSolicitada = dto.QuantidadeSolicitada,
                            QuantidadeRecebida = 0,
                            Status = StatusPedidoCompra.Pendente,
                            DataSolicitacao = DateTimeHelper.UTCBrazilNow(),
                            Observacao = string.IsNullOrWhiteSpace(dto.Observacao)
                                ? $"Pedido criado manualmente para a ordem {ordem.Numero}."
                                : dto.Observacao.Trim()
                        },
                        token);

                    pedido.Peca = peca;

                    await RegistrarHistoricoAsync(
                        ordem.Id,
                        TipoEventoOrdemServico.PedidoCompraGerado,
                        ordem.Status,
                        ordem.Status,
                        $"Pedido de compra {pedido.Id} criado manualmente para a peca {peca.Nome}. Quantidade solicitada: {dto.QuantidadeSolicitada}.",
                        token);

                    _logger.LogInformation(LogTemplate.End, LoggerName, $"Pedido de compra {pedido.Id} criado manualmente com sucesso.");
                    return MapToDto(pedido);
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CriarAsync), ex.Message);
            throw;
        }
    }

    public async Task<PagedResultDto<PedidoCompraDto>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ListarAsync), "Consultando pedidos de compra de forma paginada");
            var totalItems = await _pedidoCompraRepository.ContarAsync(cancellationToken);
            var pedidos = await _pedidoCompraRepository.ObterPaginadoAsync(page, pageSize, cancellationToken);

            var items = pedidos.Select(MapToDto).ToArray();
            var resultado = new PagedResultDto<PedidoCompraDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            _logger.LogInformation(LogTemplate.End, LoggerName, $"Consulta paginada de pedidos de compra concluida. Total de registros: {totalItems}");
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(ListarAsync), ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<PedidoCompraDto>> ListarPorOrdemDeServicoAsync(int ordemDeServicoId, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ListarPorOrdemDeServicoAsync), "Consultando pedidos de compra por ordem de servico");
            var pedidos = await _pedidoCompraRepository.ObterPorOrdemDeServicoAsync(ordemDeServicoId, cancellationToken);
            var resultado = pedidos.Select(MapToDto).ToArray();
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Consulta de pedidos de compra concluida. Total de registros: {resultado.Length}");
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(ListarPorOrdemDeServicoAsync), ex.Message);
            throw;
        }
    }

    public async Task<PedidoCompraDto> RegistrarRecebimentoAsync(int pedidoCompraId, ReceberPedidoCompraDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            return await _transactionManager.ExecuteAsync(
                async token =>
                {
                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(RegistrarRecebimentoAsync), "Consultando pedido de compra para recebimento");
                    var pedido = await _pedidoCompraRepository.ObterPorIdAsync(pedidoCompraId, token);
                    if (pedido == null)
                    {
                        _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(RegistrarRecebimentoAsync), "Pedido de compra nao encontrado para recebimento");
                        throw new KeyNotFoundException($"Pedido de compra com ID {pedidoCompraId} nao encontrado.");
                    }

                    var peca = await _pecaRepository.ObterPorIdAsync(pedido.PecaId, token);
                    if (peca == null)
                    {
                        throw new KeyNotFoundException($"Peca com ID {pedido.PecaId} nao encontrada para recebimento do pedido.");
                    }

                    var estoqueAnterior = peca.QuantidadeEstoque;
                    pedido.RegistrarRecebimento(dto.QuantidadeRecebida);
                    peca.ReporEstoque(dto.QuantidadeRecebida);

                    await _pedidoCompraRepository.AtualizarAsync(pedido, token);
                    await _pecaRepository.AtualizarAsync(peca, token);
                    await _movimentacaoEstoqueRepository.CriarAsync(
                        new MovimentacaoEstoque
                        {
                            PecaId = peca.Id,
                            OrdemDeServicoId = pedido.OrdemDeServicoId,
                            PedidoCompraId = pedido.Id,
                            TipoMovimentacao = TipoMovimentacaoEstoque.EntradaPorPedidoCompra,
                            Quantidade = dto.QuantidadeRecebida,
                            QuantidadeAnterior = estoqueAnterior,
                            QuantidadePosterior = peca.QuantidadeEstoque,
                            Descricao = $"Entrada de estoque por recebimento do pedido de compra {pedido.Id}.",
                            DataMovimentacao = DateTimeHelper.UTCBrazilNow()
                        },
                        token);

                    await RegistrarHistoricoAsync(
                        pedido.OrdemDeServicoId,
                        TipoEventoOrdemServico.PedidoCompraRecebido,
                        null,
                        null,
                        $"Pedido de compra {pedido.Id} recebido para a peca {peca.Nome}. Quantidade recebida: {dto.QuantidadeRecebida}.",
                        token);
                    await RegistrarHistoricoAsync(
                        pedido.OrdemDeServicoId,
                        TipoEventoOrdemServico.EstoqueReposto,
                        null,
                        null,
                        $"Estoque reposto para a peca {peca.Nome}. Saldo atual: {peca.QuantidadeEstoque}.",
                        token);

                    _logger.LogInformation(LogTemplate.End, LoggerName, $"Recebimento do pedido de compra {pedido.Id} registrado com sucesso.");
                    return MapToDto(pedido);
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(RegistrarRecebimentoAsync), ex.Message);
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
            new OrdemServicoHistorico
            {
                OrdemDeServicoId = ordemDeServicoId,
                UsuarioId = usuarioAtual.UsuarioId,
                UsuarioNome = usuarioAtual.UsuarioNome,
                StatusAnterior = statusAnterior,
                StatusNovo = statusNovo,
                TipoEvento = tipoEvento,
                Descricao = descricao,
                DataEvento = DateTimeHelper.UTCBrazilNow()
            },
            cancellationToken);
    }

    private static PedidoCompraDto MapToDto(PedidoCompra pedidoCompra)
    {
        return new PedidoCompraDto
        {
            Id = pedidoCompra.Id,
            OrdemDeServicoId = pedidoCompra.OrdemDeServicoId,
            PecaId = pedidoCompra.PecaId,
            NomePeca = pedidoCompra.Peca?.Nome ?? string.Empty,
            MarcaPeca = pedidoCompra.Peca?.Marca ?? string.Empty,
            ModeloPeca = pedidoCompra.Peca?.Modelo ?? string.Empty,
            QuantidadeSolicitada = pedidoCompra.QuantidadeSolicitada,
            QuantidadeRecebida = pedidoCompra.QuantidadeRecebida,
            Status = pedidoCompra.Status.ToString(),
            DataSolicitacao = pedidoCompra.DataSolicitacao,
            DataRecebimento = pedidoCompra.DataRecebimento,
            Observacao = pedidoCompra.Observacao
        };
    }
}
