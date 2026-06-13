using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Pecas;

public sealed class CriarPecaCommandHandler : IRequestHandler<CriarPecaCommand, PecaResult>
{
    private const string LoggerName = nameof(CriarPecaCommandHandler);
    private readonly IPecaRepository _pecaRepository;
    private readonly ILogger _logger;

    public CriarPecaCommandHandler(IPecaRepository pecaRepository, ILoggerFactory loggerFactory)
    {
        _pecaRepository = pecaRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<PecaResult> Handle(CriarPecaCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Persistindo nova peca");
            var peca = Peca.Criar(command.Nome, command.Marca, command.Modelo, command.Preco, command.QuantidadeEstoque);

            var pecaCriada = await _pecaRepository.CriarAsync(peca, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Peca criada com sucesso.");
            return pecaCriada.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
