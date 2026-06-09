using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Pecas;

public sealed class AtualizarPecaCommandHandler : IRequestHandler<AtualizarPecaCommand, PecaResult>
{
    private const string LoggerName = nameof(AtualizarPecaCommandHandler);
    private readonly IPecaRepository _pecaRepository;
    private readonly ILogger _logger;

    public AtualizarPecaCommandHandler(IPecaRepository pecaRepository, ILoggerFactory loggerFactory)
    {
        _pecaRepository = pecaRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<PecaResult> Handle(AtualizarPecaCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando peca para atualizacao");
            var peca = await _pecaRepository.ObterPorIdAsync(command.Id, cancellationToken);
            if (peca == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Peca nao encontrada para atualizacao");
                throw new ServiceNotFoundException($"Peca com ID {command.Id} nao encontrada.");
            }

            peca.AtualizarDados(command.Nome, command.Marca, command.Modelo, command.Preco, command.QuantidadeEstoque);

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Persistindo atualizacao da peca");
            var pecaAtualizada = await _pecaRepository.AtualizarAsync(peca, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Peca atualizada com sucesso.");
            return pecaAtualizada.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
