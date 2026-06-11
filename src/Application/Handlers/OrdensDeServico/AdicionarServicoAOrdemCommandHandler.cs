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

public sealed class AdicionarServicoAOrdemCommandHandler : IRequestHandler<AdicionarServicoAOrdemCommand, OrdemDeServicoDto>
{
    private const string LoggerName = nameof(AdicionarServicoAOrdemCommandHandler);
    private readonly OrdemDeServicoHandlerDependencies _dependencies;
    private readonly ILogger _logger;

    public AdicionarServicoAOrdemCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
    {
        _dependencies = dependencies;
        _logger = dependencies.LoggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoDto> Handle(AdicionarServicoAOrdemCommand command, CancellationToken cancellationToken)
    {
        return AdicionarServicoAsync(command.OrdemDeServicoId, new AdicionarServicoAOrdemDto { ServicoId = command.ServicoId }, cancellationToken);
    }

private async Task<OrdemDeServicoDto> AdicionarServicoAsync(int id, AdicionarServicoAOrdemDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AdicionarServicoAsync), "Consultando ordem de servico e servico para composicao do orcamento");
            var ordem = await _dependencies.OrdemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarServicoAsync), "Ordem de servico nao encontrada para adicionar servico");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            if (ordem.Servicos.Any(x => x.ServicoId == dto.ServicoId))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarServicoAsync), "Servico ja adicionado a ordem de servico");
                throw new ServiceValidationException("O servico informado ja foi adicionado a ordem de servico.");
            }

            var servico = await _dependencies.ServicoRepository.ObterPorIdAsync(dto.ServicoId, cancellationToken);
            if (servico == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarServicoAsync), "Servico nao encontrado para composicao do orcamento");
                throw new ServiceNotFoundException($"Servico com ID {dto.ServicoId} nao encontrado.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AdicionarServicoAsync), "Adicionando servico a ordem");
            var eventoServicoAdicionado = ordem.AdicionarServicoComEvento(servico);
            var ordemAtualizada = await _dependencies.OrdemRepository.AtualizarAsync(ordem, cancellationToken);
            await _dependencies.HistoricoService.RegistrarAsync(
                ordemAtualizada,
                eventoServicoAdicionado.TipoEvento,
                eventoServicoAdicionado.StatusAnterior,
                eventoServicoAdicionado.StatusNovo,
                eventoServicoAdicionado.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Servico adicionado com sucesso a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AdicionarServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
