using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
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

public sealed class RemoverPecaDaOrdemCommandHandler : IRequestHandler<RemoverPecaDaOrdemCommand, OrdemDeServicoResult>
{
    private const string LoggerName = nameof(RemoverPecaDaOrdemCommandHandler);
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly ILogger _logger;

    public RemoverPecaDaOrdemCommandHandler(
        OrdemDeServicoHistoricoService historicoService,
        IOrdemDeServicoRepository ordemRepository,
        IPecaRepository pecaRepository,
        ILoggerFactory loggerFactory)
    {
        _historicoService = historicoService;
        _ordemRepository = ordemRepository;
        _pecaRepository = pecaRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoResult> Handle(RemoverPecaDaOrdemCommand command, CancellationToken cancellationToken)
    {
        return RemoverPecaAsync(command.OrdemDeServicoId, command.PecaId, cancellationToken);
    }

    private async Task<OrdemDeServicoResult> RemoverPecaAsync(int id, int pecaId, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(RemoverPecaAsync), "Consultando ordem de servico para remover peca");
            var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordem == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(RemoverPecaAsync), "Ordem de servico nao encontrada para remover peca");
                throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
            }

            var peca = ordem.Pecas.FirstOrDefault(x => x.PecaId == pecaId)?.Peca
                ?? await _pecaRepository.ObterPorIdAsync(pecaId, cancellationToken);

            var eventoPecaRemovida = ordem.RemoverPecaComEvento(pecaId, peca?.Nome ?? pecaId.ToString());
            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoPecaRemovida.TipoEvento,
                eventoPecaRemovida.StatusAnterior,
                eventoPecaRemovida.StatusNovo,
                eventoPecaRemovida.Descricao,
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Peca removida com sucesso da ordem {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToResult(ordemAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(RemoverPecaAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}


