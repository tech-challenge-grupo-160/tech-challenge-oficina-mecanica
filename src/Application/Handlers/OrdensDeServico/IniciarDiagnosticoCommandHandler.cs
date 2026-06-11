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

public sealed class IniciarDiagnosticoCommandHandler : IRequestHandler<IniciarDiagnosticoCommand, OrdemDeServicoDto>
{
    private const string LoggerName = nameof(IniciarDiagnosticoCommandHandler);
    private readonly OrdemDeServicoHandlerDependencies _dependencies;
    private readonly ILogger _logger;

    public IniciarDiagnosticoCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
    {
        _dependencies = dependencies;
        _logger = dependencies.LoggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoDto> Handle(IniciarDiagnosticoCommand command, CancellationToken cancellationToken)
    {
        return IniciarDiagnosticoAsync(command.Id, cancellationToken);
    }

private async Task<OrdemDeServicoDto> IniciarDiagnosticoAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(IniciarDiagnosticoAsync), "Consultando ordem de servico para iniciar diagnostico");
            var ordem = await _dependencies.OrdemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(IniciarDiagnosticoAsync), "Ordem de servico nao encontrada para iniciar diagnostico");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(IniciarDiagnosticoAsync), "Alterando status da ordem para EmDiagnostico");
            var eventoDiagnosticoIniciado = ordem.IniciarDiagnostico();
            var ordemAtualizada = await _dependencies.OrdemRepository.AtualizarAsync(ordem, cancellationToken);
            await _dependencies.HistoricoService.RegistrarAsync(
                ordemAtualizada,
                eventoDiagnosticoIniciado.TipoEvento,
                eventoDiagnosticoIniciado.StatusAnterior,
                eventoDiagnosticoIniciado.StatusNovo,
                eventoDiagnosticoIniciado.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Diagnostico iniciado com sucesso para a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(IniciarDiagnosticoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
