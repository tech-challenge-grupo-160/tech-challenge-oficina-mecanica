using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class AprovarOrdemDeServicoCommandHandler : IRequestHandler<AprovarOrdemDeServicoCommand, OrdemDeServicoResult>
{
    private const string LoggerName = nameof(AprovarOrdemDeServicoCommandHandler);
    private readonly OrdemDeServicoEstoqueService _estoqueService;
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger _logger;

    public AprovarOrdemDeServicoCommandHandler(
        OrdemDeServicoEstoqueService estoqueService,
        OrdemDeServicoHistoricoService historicoService,
        IOrdemDeServicoRepository ordemRepository,
        ITransactionManager transactionManager,
        ILoggerFactory loggerFactory)
    {
        _estoqueService = estoqueService;
        _historicoService = historicoService;
        _ordemRepository = ordemRepository;
        _transactionManager = transactionManager;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoResult> Handle(AprovarOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        return AprovarAsync(command.Id, cancellationToken);
    }

    private async Task<OrdemDeServicoResult> AprovarAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            var ordemAtualizada = await _transactionManager.ExecuteAsync(
                async token =>
                {
                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AprovarAsync), "Consultando ordem de servico para aprovacao do orcamento");
                    var ordem = await _ordemRepository.ObterPorIdAsync(id, token);
                    if (ordem == null)
                    {
                        _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AprovarAsync), "Ordem de servico nao encontrada para aprovacao do orcamento");
                        throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
                    }

                    if (ordem.Status == StatusOrdemDeServico.AguardandoEstoque)
                    {
                        throw new InvalidOperationException("A ordem de servico esta aguardando estoque. Use a rota de liberacao de execucao apos reposicao do estoque.");
                    }

                    if (ordem.Status != StatusOrdemDeServico.AguardandoAprovacao)
                    {
                        throw new InvalidOperationException($"A ordem de servico nao pode ser aprovada no status atual: {ordem.Status}");
                    }

                    var faltasEstoque = await _estoqueService.ObterFaltasAsync(ordem, token);
                    if (faltasEstoque.Count > 0)
                    {
                        _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AprovarAsync), "Bloqueando aprovacao por falta de estoque e gerando pedidos de compra");
                        var eventoBloqueio = ordem.BloquearPorFaltaEstoqueComEvento(OrdemDeServicoEstoqueService.FormatarFaltas(faltasEstoque));
                        var ordemBloqueada = await _ordemRepository.AtualizarAsync(ordem, token);
                        await _historicoService.RegistrarAsync(
                            ordemBloqueada,
                            eventoBloqueio.TipoEvento,
                            eventoBloqueio.StatusAnterior,
                            eventoBloqueio.StatusNovo,
                            eventoBloqueio.Descricao,
                            token);

                        foreach (var falta in faltasEstoque)
                        {
                            await _estoqueService.CriarOuAtualizarPedidoCompraAsync(ordemBloqueada, falta.Peca, falta.QuantidadeFaltante, token);
                        }

                        return ordemBloqueada;
                    }

                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AprovarAsync), "Baixando estoque das pecas e liberando ordem para execucao");
                    await _estoqueService.BaixarEstoqueDaOrdemAsync(ordem, token);
                    var eventoAprovacao = ordem.LiberarExecucaoComEvento();
                    var ordemExecutando = await _ordemRepository.AtualizarAsync(ordem, token);
                    await _historicoService.RegistrarAsync(
                        ordemExecutando,
                        eventoAprovacao.TipoEvento,
                        eventoAprovacao.StatusAnterior,
                        eventoAprovacao.StatusNovo,
                        eventoAprovacao.Descricao,
                        token);
                    return ordemExecutando;
                },
                cancellationToken);

            _logger.LogInformation(LogTemplate.End, LoggerName, $"Orcamento aprovado com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToResult(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AprovarAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}


