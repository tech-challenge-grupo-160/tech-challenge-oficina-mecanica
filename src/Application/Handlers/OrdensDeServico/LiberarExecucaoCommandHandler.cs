using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class LiberarExecucaoCommandHandler : IRequestHandler<LiberarExecucaoCommand, OrdemDeServicoResult>
{
    private const string LoggerName = nameof(LiberarExecucaoCommandHandler);
    private readonly OrdemDeServicoEstoqueService _estoqueService;
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger _logger;

    public LiberarExecucaoCommandHandler(
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

    public Task<OrdemDeServicoResult> Handle(LiberarExecucaoCommand command, CancellationToken cancellationToken)
    {
        return LiberarExecucaoAsync(command.Id, cancellationToken);
    }

    private async Task<OrdemDeServicoResult> LiberarExecucaoAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            var ordemAtualizada = await _transactionManager.ExecuteAsync(
                async token =>
                {
                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(LiberarExecucaoAsync), "Consultando ordem de servico aguardando estoque para liberacao de execucao");
                    var ordem = await _ordemRepository.ObterPorIdAsync(id, token);
                    if (ordem == null)
                    {
                        _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(LiberarExecucaoAsync), "Ordem de servico nao encontrada para liberacao de execucao");
                        throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
                    }

                    if (ordem.Status != StatusOrdemDeServico.AguardandoEstoque)
                    {
                        throw new InvalidOperationException($"A ordem de servico so pode ser liberada para execucao quando estiver aguardando estoque. Status atual: {ordem.Status}");
                    }

                    var faltasEstoque = await _estoqueService.ObterFaltasAsync(ordem, token);
                    if (faltasEstoque.Count > 0)
                    {
                        throw new InvalidOperationException($"Estoque indisponivel para liberar execucao da ordem de servico: {OrdemDeServicoEstoqueService.FormatarFaltas(faltasEstoque)}.");
                    }

                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(LiberarExecucaoAsync), "Baixando estoque das pecas e liberando ordem para execucao");
                    await _estoqueService.BaixarEstoqueDaOrdemAsync(ordem, token);
                    var eventoLiberacao = ordem.LiberarExecucaoComEvento();
                    var ordemExecutando = await _ordemRepository.AtualizarAsync(ordem, token);
                    await _historicoService.RegistrarAsync(
                        ordemExecutando,
                        eventoLiberacao.TipoEvento,
                        eventoLiberacao.StatusAnterior,
                        eventoLiberacao.StatusNovo,
                        eventoLiberacao.Descricao,
                        token);
                    return ordemExecutando;
                },
                cancellationToken);

            _logger.LogInformation(LogTemplate.End, LoggerName, $"Execucao liberada com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToResult(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(LiberarExecucaoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}


