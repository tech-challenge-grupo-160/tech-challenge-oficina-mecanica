using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ResponderOrdemDeServicoCommandHandler : IRequestHandler<ResponderOrdemDeServicoCommand, OrdemDeServicoResult>
{
    private const string LoggerName = nameof(ResponderOrdemDeServicoCommandHandler);
    private const string MotivoRecusaPadrao = "Ordem de servico recusada pelo cliente.";
    private readonly OrdemDeServicoEstoqueService _estoqueService;
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger _logger;

    public ResponderOrdemDeServicoCommandHandler(
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

    public Task<OrdemDeServicoResult> Handle(ResponderOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        return ResponderAsync(command, cancellationToken);
    }

    private async Task<OrdemDeServicoResult> ResponderAsync(ResponderOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            var ordemAtualizada = await _transactionManager.ExecuteAsync(
                async token =>
                {
                    _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(ResponderAsync), "Consultando ordem de servico para resposta externa da ordem");
                    var ordem = await _ordemRepository.ObterPorIdAsync(command.Id, token);
                    if (ordem is null)
                    {
                        _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(ResponderAsync), "Ordem de servico nao encontrada para resposta externa da ordem");
                        throw new ServiceNotFoundException("Acompanhamento nao encontrado.");
                    }

                    ValidarTokenAcompanhamento(ordem, command.TrackingToken);
                    ValidarStatusParaRespostaOrdem(ordem);

                    return command.Aprovado
                        ? await AprovarAsync(ordem, token)
                        : await RecusarAsync(ordem, command.MotivoRecusa, token);
                },
                cancellationToken);

            _logger.LogInformation(LogTemplate.End, LoggerName, $"Resposta externa da ordem registrada para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToResult(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(ResponderAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    private void ValidarTokenAcompanhamento(OrdemDeServico ordem, string trackingToken)
    {
        var tokenHash = StringHelper.ToSha256Hash(trackingToken.Trim());
        if (!StringHelper.FixedTimeEqualsHex(tokenHash, ordem.TokenAcompanhamentoHash))
        {
            _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(ValidarTokenAcompanhamento), "Token de acompanhamento invalido para resposta externa da ordem");
            throw new ServiceNotFoundException("Acompanhamento nao encontrado.");
        }
    }

    private static void ValidarStatusParaRespostaOrdem(OrdemDeServico ordem)
    {
        if (ordem.Status != StatusOrdemDeServico.AguardandoAprovacao)
        {
            throw new InvalidOperationException(
                $"A resposta da ordem so pode alterar a ordem quando estiver no status {StatusOrdemDeServico.AguardandoAprovacao}. Status atual: {ordem.Status}");
        }
    }

    private async Task<OrdemDeServico> AprovarAsync(OrdemDeServico ordem, CancellationToken cancellationToken)
    {
        var faltasEstoque = await _estoqueService.ObterFaltasAsync(ordem, cancellationToken);
        if (faltasEstoque.Count > 0)
        {
            var eventoBloqueio = ordem.BloquearPorFaltaEstoqueComEvento(OrdemDeServicoEstoqueService.FormatarFaltas(faltasEstoque));
            var ordemBloqueada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemBloqueada,
                eventoBloqueio.TipoEvento,
                eventoBloqueio.StatusAnterior,
                eventoBloqueio.StatusNovo,
                eventoBloqueio.Descricao,
                cancellationToken);

            foreach (var falta in faltasEstoque)
            {
                await _estoqueService.CriarOuAtualizarPedidoCompraAsync(ordemBloqueada, falta.Peca, falta.QuantidadeFaltante, cancellationToken);
            }

            return ordemBloqueada;
        }

        await _estoqueService.BaixarEstoqueDaOrdemAsync(ordem, cancellationToken);
        var eventoAprovacao = ordem.LiberarExecucaoComEvento();
        var ordemExecutando = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
        await _historicoService.RegistrarAsync(
            ordemExecutando,
            eventoAprovacao.TipoEvento,
            eventoAprovacao.StatusAnterior,
            eventoAprovacao.StatusNovo,
            eventoAprovacao.Descricao,
            cancellationToken);
        return ordemExecutando;
    }

    private async Task<OrdemDeServico> RecusarAsync(OrdemDeServico ordem, string? motivoRecusa, CancellationToken cancellationToken)
    {
        var motivo = string.IsNullOrWhiteSpace(motivoRecusa) ? MotivoRecusaPadrao : motivoRecusa.Trim();
        var eventoRecusa = ordem.CancelarComEvento(motivo);
        var ordemCancelada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
        await _historicoService.RegistrarAsync(
            ordemCancelada,
            eventoRecusa.TipoEvento,
            eventoRecusa.StatusAnterior,
            eventoRecusa.StatusNovo,
            eventoRecusa.Descricao,
            cancellationToken);
        return ordemCancelada;
    }
}
