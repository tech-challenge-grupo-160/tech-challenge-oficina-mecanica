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

public sealed class ObterNotificacoesOrdemDeServicoQueryHandler : IRequestHandler<ObterNotificacoesOrdemDeServicoQuery, IEnumerable<NotificacaoClienteResult>>
{
    private const string LoggerName = nameof(ObterNotificacoesOrdemDeServicoQueryHandler);
    private readonly INotificacaoClienteRepository _notificacaoClienteRepository;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ILogger _logger;

    public ObterNotificacoesOrdemDeServicoQueryHandler(
        INotificacaoClienteRepository notificacaoClienteRepository,
        IOrdemDeServicoRepository ordemRepository,
        ILoggerFactory loggerFactory)
    {
        _notificacaoClienteRepository = notificacaoClienteRepository;
        _ordemRepository = ordemRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<IEnumerable<NotificacaoClienteResult>> Handle(ObterNotificacoesOrdemDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterNotificacoesAsync(query.Id, cancellationToken);
    }

    private async Task<IEnumerable<NotificacaoClienteResult>> ObterNotificacoesAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        var notificacoes = await _notificacaoClienteRepository.ObterPorOrdemDeServicoAsync(id, cancellationToken);
        return notificacoes.Select(OrdemDeServicoMapper.ToResult);
    }
}


