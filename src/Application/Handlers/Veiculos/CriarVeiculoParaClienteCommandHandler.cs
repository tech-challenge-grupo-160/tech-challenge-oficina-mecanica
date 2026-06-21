using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Veiculos;

public sealed class CriarVeiculoParaClienteCommandHandler : IRequestHandler<CriarVeiculoParaClienteCommand, VeiculoResult>
{
    private const string LoggerName = nameof(CriarVeiculoParaClienteCommandHandler);
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger _logger;

    public CriarVeiculoParaClienteCommandHandler(
        IVeiculoRepository veiculoRepository,
        IClienteRepository clienteRepository,
        ILoggerFactory loggerFactory)
    {
        _veiculoRepository = veiculoRepository;
        _clienteRepository = clienteRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<VeiculoResult> Handle(CriarVeiculoParaClienteCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Obtendo cliente por documento");
            var cliente = await ObterClientePorDocumentoAsync(command.CpfCnpj, cancellationToken);
            var veiculoCriado = await CriarVeiculoInternoAsync(command.Placa, command.Marca, command.Modelo, command.Ano, cliente.Id, cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, "Veiculo criado para o cliente com sucesso.");
            return veiculoCriado.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    private async Task<Cliente> ObterClientePorDocumentoAsync(string cpfCnpj, CancellationToken cancellationToken)
    {
        var documento = Documento.Parse(cpfCnpj);
        var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(documento, cancellationToken);
        if (cliente == null)
        {
            throw new ServiceNotFoundException($"Cliente com CPF/CNPJ {cpfCnpj} nao encontrado.");
        }

        return cliente;
    }

    private async Task<Veiculo> CriarVeiculoInternoAsync(
        string placaInformada,
        string marca,
        string modelo,
        int ano,
        int clienteId,
        CancellationToken cancellationToken)
    {
        var placa = PlacaVeiculo.Parse(placaInformada);
        var veiculoExistente = await _veiculoRepository.ObterPorPlacaAsync(placa, cancellationToken);
        if (veiculoExistente != null)
        {
            throw new ServiceValidationException("Veiculo com esta placa ja existe.");
        }

        var veiculo = Veiculo.Criar(placa, marca, modelo, ano, clienteId);
        return await _veiculoRepository.CriarAsync(veiculo, cancellationToken);
    }
}
