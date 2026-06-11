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

public sealed class CancelarOrdemDeServicoCommandHandler : IRequestHandler<CancelarOrdemDeServicoCommand, OrdemDeServicoDto>
{
    private const string LoggerName = nameof(CancelarOrdemDeServicoCommandHandler);
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ILogger _logger;

    public CancelarOrdemDeServicoCommandHandler(
        OrdemDeServicoHistoricoService historicoService,
        IOrdemDeServicoRepository ordemRepository,
        ILoggerFactory loggerFactory)
    {
        _historicoService = historicoService;
        _ordemRepository = ordemRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoDto> Handle(CancelarOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        return CancelarAsync(command.Id, new CancelarOrdemDeServicoDto { MotivoCancelamento = command.MotivoCancelamento }, cancellationToken);
    }

    private async Task<OrdemDeServicoDto> CancelarAsync(int id, CancelarOrdemDeServicoDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CancelarAsync), "Consultando ordem de servico para cancelamento");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CancelarAsync), "Ordem de servico nao encontrada para cancelamento");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CancelarAsync), "Cancelando ordem de servico com motivo informado");
            var eventoCancelamento = ordem.CancelarComEvento(dto.MotivoCancelamento);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoCancelamento.TipoEvento,
                eventoCancelamento.StatusAnterior,
                eventoCancelamento.StatusNovo,
                eventoCancelamento.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Ordem de servico cancelada com sucesso. Numero: {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CancelarAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}


