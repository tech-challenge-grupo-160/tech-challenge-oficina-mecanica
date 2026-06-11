using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
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

public sealed class AprovarOrdemDeServicoCommandHandler : IRequestHandler<AprovarOrdemDeServicoCommand, OrdemDeServicoDto>
{
    private const string LoggerName = nameof(AprovarOrdemDeServicoCommandHandler);
    private readonly OrdemDeServicoHandlerDependencies _dependencies;
    private readonly ILogger _logger;

    public AprovarOrdemDeServicoCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
    {
        _dependencies = dependencies;
        _logger = dependencies.LoggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoDto> Handle(AprovarOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        return AprovarAsync(command.Id, cancellationToken);
    }

private async Task<OrdemDeServicoDto> AprovarAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            var ordemAtualizada = await _dependencies.TransactionManager.ExecuteAsync(
                async token =>
                {
                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AprovarAsync), "Consultando ordem de servico para aprovacao do orcamento");
                    var ordem = await _dependencies.OrdemRepository.ObterPorIdAsync(id, token);
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

                    var faltasEstoque = await _dependencies.EstoqueService.ObterFaltasAsync(ordem, token);
                    if (faltasEstoque.Count > 0)
                    {
                        _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AprovarAsync), "Bloqueando aprovacao por falta de estoque e gerando pedidos de compra");
                        var eventoBloqueio = ordem.BloquearPorFaltaEstoqueComEvento(OrdemDeServicoEstoqueService.FormatarFaltas(faltasEstoque));
                        var ordemBloqueada = await _dependencies.OrdemRepository.AtualizarAsync(ordem, token);
                        await _dependencies.HistoricoService.RegistrarAsync(
                            ordemBloqueada,
                            eventoBloqueio.TipoEvento,
                            eventoBloqueio.StatusAnterior,
                            eventoBloqueio.StatusNovo,
                            eventoBloqueio.Descricao,
                            token);

                        foreach (var falta in faltasEstoque)
                        {
                            await _dependencies.EstoqueService.CriarOuAtualizarPedidoCompraAsync(ordemBloqueada, falta.Peca, falta.QuantidadeFaltante, token);
                        }

                        return ordemBloqueada;
                    }

                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AprovarAsync), "Baixando estoque das pecas e liberando ordem para execucao");
                    await _dependencies.EstoqueService.BaixarEstoqueDaOrdemAsync(ordem, token);
                    var eventoAprovacao = ordem.LiberarExecucaoComEvento();
                    var ordemExecutando = await _dependencies.OrdemRepository.AtualizarAsync(ordem, token);
                    await _dependencies.HistoricoService.RegistrarAsync(
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
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AprovarAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
