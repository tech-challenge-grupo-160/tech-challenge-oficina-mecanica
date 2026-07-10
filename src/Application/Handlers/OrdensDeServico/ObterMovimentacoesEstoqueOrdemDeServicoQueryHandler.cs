using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
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

public sealed class ObterMovimentacoesEstoqueOrdemDeServicoQueryHandler : IRequestHandler<ObterMovimentacoesEstoqueOrdemDeServicoQuery, IEnumerable<MovimentacoesEstoquePorPecaResult>>
{
    private const string LoggerName = nameof(ObterMovimentacoesEstoqueOrdemDeServicoQueryHandler);
    private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly ILogger _logger;

    public ObterMovimentacoesEstoqueOrdemDeServicoQueryHandler(
        IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
        IOrdemDeServicoRepository ordemRepository,
        IPecaRepository pecaRepository,
        ILoggerFactory loggerFactory)
    {
        _movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
        _ordemRepository = ordemRepository;
        _pecaRepository = pecaRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<IEnumerable<MovimentacoesEstoquePorPecaResult>> Handle(ObterMovimentacoesEstoqueOrdemDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterMovimentacoesEstoqueAsync(query.Id, cancellationToken);
    }

    private async Task<IEnumerable<MovimentacoesEstoquePorPecaResult>> ObterMovimentacoesEstoqueAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        var movimentacoes = (await _movimentacaoEstoqueRepository.ObterPorOrdemDeServicoAsync(id, cancellationToken))
            .Select(OrdemDeServicoMapper.ToResult)
            .GroupBy(x => x.PecaId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var grupos = new List<MovimentacoesEstoquePorPecaResult>();

        foreach (var item in ordem.Pecas.OrderBy(x => x.PecaId))
        {
            var peca = item.Peca ?? await _pecaRepository.ObterPorIdAsync(item.PecaId, cancellationToken);
            var movimentacoesDaPeca = movimentacoes.TryGetValue(item.PecaId, out var valores)
                ? valores
                : new List<MovimentacaoEstoqueResult>();

            grupos.Add(new MovimentacoesEstoquePorPecaResult
            {
                PecaId = item.PecaId,
                NomePeca = peca?.Nome ?? movimentacoesDaPeca.FirstOrDefault()?.NomePeca ?? string.Empty,
                MarcaPeca = peca?.Marca ?? string.Empty,
                ModeloPeca = peca?.Modelo ?? string.Empty,
                QuantidadeNaOrdem = item.Quantidade,
                TotalMovimentacoes = movimentacoesDaPeca.Count,
                Movimentacoes = movimentacoesDaPeca
            });
        }

        return grupos;
    }
}


