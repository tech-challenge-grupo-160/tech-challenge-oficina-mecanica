using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Servicos;

public sealed class CriarServicoCommandHandler : IRequestHandler<CriarServicoCommand, ServicoResult>
{
    private const string LoggerName = nameof(CriarServicoCommandHandler);
    private readonly IServicoRepository _servicoRepository;
    private readonly ILogger _logger;

    public CriarServicoCommandHandler(IServicoRepository servicoRepository, ILoggerFactory loggerFactory)
    {
        _servicoRepository = servicoRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<ServicoResult> Handle(CriarServicoCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Persistindo novo servico");
            var servico = Servico.Criar(command.Nome, command.Descricao, command.Preco, command.TempoEstimado);

            var servicoCriado = await _servicoRepository.CriarAsync(servico, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Servico criado com sucesso.");
            return servicoCriado.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
