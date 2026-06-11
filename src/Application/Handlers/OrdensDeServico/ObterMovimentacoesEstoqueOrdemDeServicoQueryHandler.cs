using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
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

public sealed class ObterMovimentacoesEstoqueOrdemDeServicoQueryHandler : IRequestHandler<ObterMovimentacoesEstoqueOrdemDeServicoQuery, IEnumerable<MovimentacoesEstoquePorPecaDto>>
{
    private const string LoggerName = nameof(ObterMovimentacoesEstoqueOrdemDeServicoQueryHandler);
    private readonly OrdemDeServicoHandlerDependencies _dependencies;
    private readonly ILogger _logger;

    public ObterMovimentacoesEstoqueOrdemDeServicoQueryHandler(OrdemDeServicoHandlerDependencies dependencies)
    {
        _dependencies = dependencies;
        _logger = dependencies.LoggerFactory.CreateLogger(LoggerName);
    }

    public Task<IEnumerable<MovimentacoesEstoquePorPecaDto>> Handle(ObterMovimentacoesEstoqueOrdemDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterMovimentacoesEstoqueAsync(query.Id, cancellationToken);
    }

private async Task<IEnumerable<MovimentacoesEstoquePorPecaDto>> ObterMovimentacoesEstoqueAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _dependencies.OrdemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new KeyNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        var movimentacoes = (await _dependencies.MovimentacaoEstoqueRepository.ObterPorOrdemDeServicoAsync(id, cancellationToken))
            .Select(OrdemDeServicoMapper.ToDto)
            .GroupBy(x => x.PecaId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var grupos = new List<MovimentacoesEstoquePorPecaDto>();

        foreach (var item in ordem.Pecas.OrderBy(x => x.PecaId))
        {
            var peca = item.Peca ?? await _dependencies.PecaRepository.ObterPorIdAsync(item.PecaId, cancellationToken);
            var movimentacoesDaPeca = movimentacoes.TryGetValue(item.PecaId, out var valores)
                ? valores
                : new List<MovimentacaoEstoqueDto>();

            grupos.Add(new MovimentacoesEstoquePorPecaDto
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
