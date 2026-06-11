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

public sealed class RemoverServicoDaOrdemCommandHandler : IRequestHandler<RemoverServicoDaOrdemCommand, OrdemDeServicoDto>
{
    private const string LoggerName = nameof(RemoverServicoDaOrdemCommandHandler);
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly ILogger _logger;

    public RemoverServicoDaOrdemCommandHandler(
        OrdemDeServicoHistoricoService historicoService,
        IOrdemDeServicoRepository ordemRepository,
        IServicoRepository servicoRepository,
        ILoggerFactory loggerFactory)
    {
        _historicoService = historicoService;
        _ordemRepository = ordemRepository;
        _servicoRepository = servicoRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoDto> Handle(RemoverServicoDaOrdemCommand command, CancellationToken cancellationToken)
    {
        return RemoverServicoAsync(command.OrdemDeServicoId, command.ServicoId, cancellationToken);
    }

    private async Task<OrdemDeServicoDto> RemoverServicoAsync(int id, int servicoId, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(RemoverServicoAsync), "Consultando ordem de servico para remover servico");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(RemoverServicoAsync), "Ordem de servico nao encontrada para remover servico");
                throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            var servico = ordem.Servicos.FirstOrDefault(x => x.ServicoId == servicoId)?.Servico
                ?? await _servicoRepository.ObterPorIdAsync(servicoId, cancellationToken);

            var eventoServicoRemovido = ordem.RemoverServicoComEvento(servicoId, servico?.Nome ?? servicoId.ToString());
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoServicoRemovido.TipoEvento,
                eventoServicoRemovido.StatusAnterior,
                eventoServicoRemovido.StatusNovo,
                eventoServicoRemovido.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Servico removido com sucesso da ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(RemoverServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}


