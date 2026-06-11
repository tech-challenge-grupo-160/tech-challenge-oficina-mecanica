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

public sealed class FinalizarDiagnosticoCommandHandler : IRequestHandler<FinalizarDiagnosticoCommand, OrdemDeServicoDto>
{
    private const string LoggerName = nameof(FinalizarDiagnosticoCommandHandler);
    private readonly IClock _clock;
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly OrdemDeServicoNotificacaoService _notificacaoService;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ILogger _logger;

    public FinalizarDiagnosticoCommandHandler(
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

    public Task<OrdemDeServicoDto> Handle(FinalizarDiagnosticoCommand command, CancellationToken cancellationToken)
    {
        return FinalizarDiagnosticoAsync(command.Id, cancellationToken);
    }

    private async Task<OrdemDeServicoDto> FinalizarDiagnosticoAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(FinalizarDiagnosticoAsync), "Consultando ordem de servico para finalizar diagnostico");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(FinalizarDiagnosticoAsync), "Ordem de servico nao encontrada para finalizar diagnostico");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(FinalizarDiagnosticoAsync), "Validando composicao da OS e alterando status para AguardandoAprovacao");
            var eventoDiagnosticoFinalizado = ordem.FinalizarDiagnosticoComEvento(_clock.Now);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoDiagnosticoFinalizado.TipoEvento,
                eventoDiagnosticoFinalizado.StatusAnterior,
                eventoDiagnosticoFinalizado.StatusNovo,
                eventoDiagnosticoFinalizado.Descricao,
                cancellationToken);
            await _notificacaoService.RegistrarAsync(
                ordemAtualizada.Id,
                TipoNotificacaoCliente.OrcamentoDisponivel,
                CanalNotificacaoCliente.WhatsApp,
                $"Orcamento disponivel para a ordem de servico {ordemAtualizada.Numero}. Endpoint de acompanhamento: {OrdemDeServicoAcompanhamentoService.MontarEndpointAcompanhamento(ordemAtualizada.CodigoAcompanhamento)}",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Diagnostico finalizado com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(FinalizarDiagnosticoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}


