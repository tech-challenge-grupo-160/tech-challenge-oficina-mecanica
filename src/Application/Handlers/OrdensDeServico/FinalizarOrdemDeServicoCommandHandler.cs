using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
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

public sealed class FinalizarOrdemDeServicoCommandHandler : IRequestHandler<FinalizarOrdemDeServicoCommand, OrdemDeServicoResult>
{
    private const string LoggerName = nameof(FinalizarOrdemDeServicoCommandHandler);
    private readonly IClock _clock;
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly OrdemDeServicoNotificacaoService _notificacaoService;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ILogger _logger;

    public FinalizarOrdemDeServicoCommandHandler(
        IClock clock,
        OrdemDeServicoHistoricoService historicoService,
        OrdemDeServicoNotificacaoService notificacaoService,
        IOrdemDeServicoRepository ordemRepository,
        ILoggerFactory loggerFactory)
    {
        _clock = clock;
        _historicoService = historicoService;
        _notificacaoService = notificacaoService;
        _ordemRepository = ordemRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoResult> Handle(FinalizarOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        return FinalizarAsync(command.Id, cancellationToken);
    }

    private async Task<OrdemDeServicoResult> FinalizarAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(FinalizarAsync), "Consultando ordem de servico para finalizacao");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(FinalizarAsync), "Ordem de servico nao encontrada para finalizacao");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(FinalizarAsync), "Finalizando servico e alterando status para Finalizada");
            var eventoFinalizacao = ordem.FinalizarServicoComEvento(_clock.Now);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoFinalizacao.TipoEvento,
                eventoFinalizacao.StatusAnterior,
                eventoFinalizacao.StatusNovo,
                eventoFinalizacao.Descricao,
                cancellationToken);
            await _notificacaoService.RegistrarAsync(
                ordemAtualizada.Id,
                TipoNotificacaoCliente.ServicoFinalizado,
                CanalNotificacaoCliente.WhatsApp,
                $"Servico finalizado para a ordem de servico {ordemAtualizada.Numero}. Veiculo pronto para pagamento e retirada. Endpoint de acompanhamento: {OrdemDeServicoAcompanhamentoService.MontarEndpointAcompanhamento(ordemAtualizada.CodigoAcompanhamento)}",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Servico finalizado com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToResult(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(FinalizarAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}


