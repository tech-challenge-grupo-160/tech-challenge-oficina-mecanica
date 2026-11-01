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

public sealed class IniciarDiagnosticoCommandHandler : IRequestHandler<IniciarDiagnosticoCommand, OrdemDeServicoResult>
{
    private const string LoggerName = nameof(IniciarDiagnosticoCommandHandler);
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly IDiagnosticoExecucao _diagnosticoExecucao;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger _logger;

    public IniciarDiagnosticoCommandHandler(
        OrdemDeServicoHistoricoService historicoService,
        IDiagnosticoExecucao diagnosticoExecucao,
        IOrdemDeServicoRepository ordemRepository,
        ITransactionManager transactionManager,
        ILoggerFactory loggerFactory)
    {
        _historicoService = historicoService;
        _diagnosticoExecucao = diagnosticoExecucao;
        _ordemRepository = ordemRepository;
        _transactionManager = transactionManager;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoResult> Handle(IniciarDiagnosticoCommand command, CancellationToken cancellationToken)
    {
        return IniciarDiagnosticoAsync(command.Id, cancellationToken);
    }

    private async Task<OrdemDeServicoResult> IniciarDiagnosticoAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(IniciarDiagnosticoAsync), "Consultando ordem de servico para iniciar diagnostico");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(IniciarDiagnosticoAsync), "Ordem de servico nao encontrada para iniciar diagnostico");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            var repeticaoIdempotente = ordem.Status == StatusOrdemDeServico.EmDiagnostico;
            if (!repeticaoIdempotente)
            {
                ordem.ValidarInicioDiagnostico();
            }
            await _diagnosticoExecucao.IniciarAsync(
                ordem.Id,
                ordem.VeiculoId,
                ordem.DescricaoSolicitacao,
                ordem.Servicos.Select(servico => servico.ServicoId),
                ordem.Pecas.Select(peca => peca.PecaId),
                cancellationToken);

            if (repeticaoIdempotente)
            {
                _logger.LogInformation(LogTemplate.End, LoggerName, $"Diagnostico ja iniciado para a ordem {ordem.Numero}");
                return OrdemDeServicoMapper.ToResult(ordem);
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(IniciarDiagnosticoAsync), "Alterando status da ordem para EmDiagnostico");
            var ordemAtualizada = await _transactionManager.ExecuteAsync(async token =>
            {
                var eventoDiagnosticoIniciado = ordem.IniciarDiagnostico();
                var atualizada = await _ordemRepository.AtualizarAsync(ordem, token);
                await _historicoService.RegistrarAsync(
                    atualizada,
                    eventoDiagnosticoIniciado.TipoEvento,
                    eventoDiagnosticoIniciado.StatusAnterior,
                    eventoDiagnosticoIniciado.StatusNovo,
                    eventoDiagnosticoIniciado.Descricao,
                    token);
                return atualizada;
            }, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Diagnostico iniciado com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToResult(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(IniciarDiagnosticoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}


