using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
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

public sealed class AdicionarServicoAOrdemCommandHandler : IRequestHandler<AdicionarServicoAOrdemCommand, OrdemDeServicoResult>
{
    private const string LoggerName = nameof(AdicionarServicoAOrdemCommandHandler);
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly ILogger _logger;

    public AdicionarServicoAOrdemCommandHandler(
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

    public Task<OrdemDeServicoResult> Handle(AdicionarServicoAOrdemCommand command, CancellationToken cancellationToken)
    {
        return AdicionarServicoAsync(command, cancellationToken);
    }

    private async Task<OrdemDeServicoResult> AdicionarServicoAsync(AdicionarServicoAOrdemCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AdicionarServicoAsync), "Consultando ordem de servico e servico para composicao do orcamento");
            var ordem = await _ordemRepository.ObterPorIdAsync(command.OrdemDeServicoId, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarServicoAsync), "Ordem de servico nao encontrada para adicionar servico");
                throw new ServiceNotFoundException($"Ordem de servico com ID {command.OrdemDeServicoId} nao encontrada.");
            }

            if (ordem.Servicos.Any(x => x.ServicoId == command.ServicoId))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarServicoAsync), "Servico ja adicionado a ordem de servico");
                throw new ServiceValidationException("O servico informado ja foi adicionado a ordem de servico.");
            }

            var servico = await _servicoRepository.ObterPorIdAsync(command.ServicoId, cancellationToken);
            if (servico == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(AdicionarServicoAsync), "Servico nao encontrado para composicao do orcamento");
                throw new ServiceNotFoundException($"Servico com ID {command.ServicoId} nao encontrado.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(AdicionarServicoAsync), "Adicionando servico a ordem");
            var eventoServicoAdicionado = ordem.AdicionarServicoComEvento(servico);
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoServicoAdicionado.TipoEvento,
                eventoServicoAdicionado.StatusAnterior,
                eventoServicoAdicionado.StatusNovo,
                eventoServicoAdicionado.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Servico adicionado com sucesso a ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToResult(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(AdicionarServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}


