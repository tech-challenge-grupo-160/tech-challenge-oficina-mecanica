using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class AvancarStatusOrdemDeServicoCommandHandler : IRequestHandler<AvancarStatusOrdemDeServicoCommand, OrdemDeServicoResult>
{
    private const string LoggerName = nameof(AvancarStatusOrdemDeServicoCommandHandler);
    private readonly IClock _clock;
    private readonly OrdemDeServicoEstoqueService _estoqueService;
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly OrdemDeServicoNotificacaoService _notificacaoService;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger _logger;

    public AvancarStatusOrdemDeServicoCommandHandler(
        IClock clock,
        OrdemDeServicoEstoqueService estoqueService,
        OrdemDeServicoHistoricoService historicoService,
        OrdemDeServicoNotificacaoService notificacaoService,
        IOrdemDeServicoRepository ordemRepository,
        ITransactionManager transactionManager,
        ILoggerFactory loggerFactory)
    {
        _clock = clock;
        _estoqueService = estoqueService;
        _historicoService = historicoService;
        _notificacaoService = notificacaoService;
        _ordemRepository = ordemRepository;
        _transactionManager = transactionManager;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoResult> Handle(AvancarStatusOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        return AvancarStatusAsync(command.Numero, cancellationToken);
    }

    private async Task<OrdemDeServicoResult> AvancarStatusAsync(string numero, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AvancarStatusAsync), "Consultando ordem de servico por numero para avanco de status");
            var ordem = await _ordemRepository.ObterPorNumeroAsync(numero, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AvancarStatusAsync), "Ordem de servico nao encontrada para avanco de status");
                throw new ServiceNotFoundException($"Ordem de servico com numero {numero} nao encontrada.");
            }

            var ordemAtualizada = ordem.Status switch
            {
                StatusOrdemDeServico.Recebida => await AvancarDeRecebidaAsync(ordem, cancellationToken),
                StatusOrdemDeServico.EmDiagnostico => await AvancarDeEmDiagnosticoAsync(ordem, cancellationToken),
                StatusOrdemDeServico.AguardandoAprovacao => await AvancarDeAguardandoAprovacaoAsync(ordem, cancellationToken),
                StatusOrdemDeServico.EmExecucao => await AvancarDeEmExecucaoAsync(ordem, cancellationToken),
                StatusOrdemDeServico.AguardandoEstoque => throw new InvalidOperationException(
                    "A ordem de servico esta aguardando estoque. Use o endpoint PATCH {id}/liberar-execucao apos reposicao do estoque."),
                StatusOrdemDeServico.Finalizada => throw new InvalidOperationException(
                    "A ordem de servico esta finalizada. Registre o pagamento via PATCH {id}/registrar-pagamento e utilize PATCH {id}/entregar."),
                StatusOrdemDeServico.Entregue => throw new InvalidOperationException(
                    "A ordem de servico ja foi entregue. Nao ha proximo status."),
                StatusOrdemDeServico.Cancelada => throw new InvalidOperationException(
                    "A ordem de servico esta cancelada. Nao ha proximo status."),
                _ => throw new InvalidOperationException($"Status desconhecido: {ordem.Status}")
            };

            _logger.LogInformation(LogTemplate.End, LoggerName, $"Status avancado com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToResult(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AvancarStatusAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    private async Task<OrdemDeServico> AvancarDeRecebidaAsync(OrdemDeServico ordem, CancellationToken cancellationToken)
    {
        _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AvancarDeRecebidaAsync), "Avancando status de Recebida para EmDiagnostico");
        var evento = ordem.IniciarDiagnostico();
        var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
        await _historicoService.RegistrarAsync(
            ordemAtualizada,
            evento.TipoEvento,
            evento.StatusAnterior,
            evento.StatusNovo,
            evento.Descricao,
            cancellationToken);
        return ordemAtualizada;
    }

    private async Task<OrdemDeServico> AvancarDeEmDiagnosticoAsync(OrdemDeServico ordem, CancellationToken cancellationToken)
    {
        _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AvancarDeEmDiagnosticoAsync), "Avancando status de EmDiagnostico para AguardandoAprovacao");
        var evento = ordem.FinalizarDiagnosticoComEvento(_clock.Now);
        var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
        await _historicoService.RegistrarAsync(
            ordemAtualizada,
            evento.TipoEvento,
            evento.StatusAnterior,
            evento.StatusNovo,
            evento.Descricao,
            cancellationToken);
        await _notificacaoService.RegistrarAsync(
            ordemAtualizada.Id,
            TipoNotificacaoCliente.OrcamentoDisponivel,
            CanalNotificacaoCliente.WhatsApp,
            $"Orcamento disponivel para a ordem de servico {ordemAtualizada.Numero}. Acesse o acompanhamento apos autenticar com CPF/CNPJ: {OrdemDeServicoAcompanhamentoService.MontarEndpointAcompanhamento(ordemAtualizada.CodigoAcompanhamento)}",
            cancellationToken);
        return ordemAtualizada;
    }

    private async Task<OrdemDeServico> AvancarDeAguardandoAprovacaoAsync(OrdemDeServico ordem, CancellationToken cancellationToken)
    {
        _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AvancarDeAguardandoAprovacaoAsync), "Avancando status de AguardandoAprovacao com validacao de estoque");
        return await _transactionManager.ExecuteAsync(
            async token =>
            {
                var faltasEstoque = await _estoqueService.ObterFaltasAsync(ordem, token);
                if (faltasEstoque.Count > 0)
                {
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
    }

    private async Task<OrdemDeServico> AvancarDeEmExecucaoAsync(OrdemDeServico ordem, CancellationToken cancellationToken)
    {
        _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AvancarDeEmExecucaoAsync), "Avancando status de EmExecucao para Finalizada");
        var evento = ordem.FinalizarServicoComEvento(_clock.Now);
        var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
        await _historicoService.RegistrarAsync(
            ordemAtualizada,
            evento.TipoEvento,
            evento.StatusAnterior,
            evento.StatusNovo,
            evento.Descricao,
            cancellationToken);
        await _notificacaoService.RegistrarAsync(
            ordemAtualizada.Id,
            TipoNotificacaoCliente.ServicoFinalizado,
            CanalNotificacaoCliente.WhatsApp,
            $"Servico finalizado para a ordem de servico {ordemAtualizada.Numero}. Veiculo pronto para pagamento e retirada. Acompanhamento disponivel apos autenticacao: {OrdemDeServicoAcompanhamentoService.MontarEndpointAcompanhamento(ordemAtualizada.CodigoAcompanhamento)}",
            cancellationToken);
        return ordemAtualizada;
    }
}
