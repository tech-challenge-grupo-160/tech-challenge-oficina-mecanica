using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Veiculos;

public sealed class ObterVeiculoPorPlacaQueryHandler : IRequestHandler<ObterVeiculoPorPlacaQuery, VeiculoResult>
{
    private const string LoggerName = nameof(ObterVeiculoPorPlacaQueryHandler);
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly ILogger _logger;

    public ObterVeiculoPorPlacaQueryHandler(
        IVeiculoRepository veiculoRepository,
        ILoggerFactory loggerFactory)
    {
        _veiculoRepository = veiculoRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<VeiculoResult> Handle(ObterVeiculoPorPlacaQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Normalizando placa e consultando veiculo");
            var placaNormalizada = PlacaVeiculo.Parse(query.Placa).Valor;
            var veiculo = await _veiculoRepository.ObterPorPlacaAsync(placaNormalizada, cancellationToken);
            if (veiculo == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Veiculo nao encontrado para a placa informada");
                throw new ServiceNotFoundException($"Veiculo com placa {query.Placa} nao encontrado.");
            }

            _logger.LogInformation(LogTemplate.End, LoggerName, "Veiculo obtido com sucesso.");
            return veiculo.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
