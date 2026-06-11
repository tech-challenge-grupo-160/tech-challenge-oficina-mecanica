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

public sealed class AdicionarPecaAOrdemCommandHandler : IRequestHandler<AdicionarPecaAOrdemCommand, OrdemDeServicoDto>
{
    private const string LoggerName = nameof(AdicionarPecaAOrdemCommandHandler);
    private readonly OrdemDeServicoHandlerDependencies _dependencies;
    private readonly ILogger _logger;

    public AdicionarPecaAOrdemCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
    {
        _dependencies = dependencies;
        _logger = dependencies.LoggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoDto> Handle(AdicionarPecaAOrdemCommand command, CancellationToken cancellationToken)
    {
        return AdicionarPecaAsync(command.OrdemDeServicoId, new AdicionarPecaAOrdemDto { PecaId = command.PecaId, Quantidade = command.Quantidade }, cancellationToken);
    }

private async Task<OrdemDeServicoDto> AdicionarPecaAsync(int id, AdicionarPecaAOrdemDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AdicionarPecaAsync), "Consultando ordem de servico e peca para composicao do orcamento");
            var ordem = await _dependencies.OrdemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarPecaAsync), "Ordem de servico nao encontrada para adicionar peca");
                throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            var peca = await _dependencies.PecaRepository.ObterPorIdAsync(dto.PecaId, cancellationToken);
            if (peca == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarPecaAsync), "Peca nao encontrada para composicao do orcamento");
                throw new ServiceNotFoundException($"Peca com ID {dto.PecaId} nao encontrada.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AdicionarPecaAsync), "Adicionando peca a ordem");
            var eventoPecaAdicionada = ordem.AdicionarPecaComEvento(peca, dto.Quantidade);
            var ordemAtualizada = await _dependencies.OrdemRepository.AtualizarAsync(ordem, cancellationToken);
            await _dependencies.HistoricoService.RegistrarAsync(
                ordemAtualizada,
                eventoPecaAdicionada.TipoEvento,
                eventoPecaAdicionada.StatusAnterior,
                eventoPecaAdicionada.StatusNovo,
                eventoPecaAdicionada.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Peca adicionada com sucesso a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToDto(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AdicionarPecaAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
