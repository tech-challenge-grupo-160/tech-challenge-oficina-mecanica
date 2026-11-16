using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
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

public sealed class CriarOrdemDeServicoCommandHandler : IRequestHandler<CriarOrdemDeServicoCommand, OrdemDeServicoResult>
{
    private const string LoggerName = nameof(CriarOrdemDeServicoCommandHandler);
    private readonly OrdemDeServicoAcompanhamentoService _acompanhamentoService;
    private readonly IClienteRepository _clienteRepository;
    private readonly IClock _clock;
    private readonly IBusinessMetrics _businessMetrics;
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly OrdemDeServicoNotificacaoService _notificacaoService;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ICatalogoExecucao _catalogo;
    private readonly ITransactionManager _transactionManager;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly ILogger _logger;

    public CriarOrdemDeServicoCommandHandler(
        OrdemDeServicoAcompanhamentoService acompanhamentoService,
        IClienteRepository clienteRepository,
        IClock clock,
        OrdemDeServicoHistoricoService historicoService,
        OrdemDeServicoNotificacaoService notificacaoService,
        IOrdemDeServicoRepository ordemRepository,
        ICatalogoExecucao catalogo,
        ITransactionManager transactionManager,
        IVeiculoRepository veiculoRepository,
        ILoggerFactory loggerFactory,
        IBusinessMetrics businessMetrics)
    {
        _acompanhamentoService = acompanhamentoService;
        _clienteRepository = clienteRepository;
        _clock = clock;
        _businessMetrics = businessMetrics;
        _historicoService = historicoService;
        _notificacaoService = notificacaoService;
        _ordemRepository = ordemRepository;
        _catalogo = catalogo;
        _transactionManager = transactionManager;
        _veiculoRepository = veiculoRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoResult> Handle(CriarOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        return CriarOrdemDeServicoAsync(command, cancellationToken);
    }

    private async Task<OrdemDeServicoResult> CriarOrdemDeServicoAsync(CriarOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarOrdemDeServicoAsync), "Consultando cliente e veiculo informados");
            var cliente = await _clienteRepository.ObterPorIdAsync(command.ClienteId, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Cliente nao encontrado para abertura da OS");
                throw new ServiceNotFoundException($"Cliente com ID {command.ClienteId} nao encontrado.");
            }

            var veiculo = await _veiculoRepository.ObterPorIdAsync(command.VeiculoId, cancellationToken);
            if (veiculo == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Veiculo nao encontrado para abertura da OS");
                throw new ServiceNotFoundException($"Veiculo com ID {command.VeiculoId} nao encontrado.");
            }

            if (veiculo.ClienteId != command.ClienteId)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Veiculo nao pertence ao cliente informado");
                throw new ServiceValidationException("O veiculo informado nao pertence ao cliente informado.");
            }

            var existeOrdemAtiva = await _ordemRepository.ExisteOrdemAtivaPorClienteEVeiculoAsync(command.ClienteId, command.VeiculoId, cancellationToken);
            if (existeOrdemAtiva)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Ja existe ordem de servico ativa para o cliente e veiculo informados");
                throw new ServiceValidationException("Ja existe uma ordem de servico ativa para este cliente e veiculo.");
            }

            var servicoIds = command.Servicos.Select(s => s.ServicoId).ToList();
            var pecaIds = command.Pecas.Select(p => p.PecaId).ToList();
            var catalogo = await _catalogo.ObterItensAsync(servicoIds, pecaIds, cancellationToken);
            var servicosEncontrados = catalogo.Servicos;
            var servicosDict = servicosEncontrados.ToDictionary(s => s.Id);
            foreach (var id in servicoIds)
            {
                if (!servicosDict.ContainsKey(id))
                {
                    _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Servico nao encontrado para abertura da OS");
                    throw new ServiceNotFoundException($"Servico com ID {id} nao encontrado.");
                }
            }

            var servicos = servicoIds.Select(id => servicosDict[id]).ToList();

            var pecasEncontradas = catalogo.Pecas;
            var pecasDict = pecasEncontradas.ToDictionary(p => p.Id);
            foreach (var id in pecaIds)
            {
                if (!pecasDict.ContainsKey(id))
                {
                    _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Peca nao encontrada para abertura da OS");
                    throw new ServiceNotFoundException($"Peca com ID {id} nao encontrada.");
                }
            }

            var pecas = command.Pecas.Select(p => (Peca: pecasDict[p.PecaId], p.Quantidade)).ToList();

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarOrdemDeServicoAsync), "Persistindo ordem de servico em status Recebida");
            // A consulta ao catalogo termina antes da transacao de escrita local.
            var ordemAtualizada = await _transactionManager.ExecuteAsync(async token =>
            {
                var (codigoAcompanhamento, _, tokenAcompanhamentoHash) =
                    await _acompanhamentoService.GerarCredenciaisAsync(token);

                var ordem = OrdemDeServico.Criar(
                    OrdemDeServicoAcompanhamentoService.GerarNumeroTemporario(),
                    codigoAcompanhamento,
                    tokenAcompanhamentoHash,
                    command.ClienteId,
                    command.VeiculoId,
                    command.DescricaoSolicitacao,
                    command.ObservacoesRecepcao,
                    _clock.Now);

                var ordemCriada = await _ordemRepository.CriarAsync(ordem, token);
                ordemCriada.DefinirNumero(OrdemDeServicoAcompanhamentoService.GerarNumeroOrdem(ordemCriada.Id, ordemCriada.DataAbertura));

                var eventosItens = new List<OrdemDeServicoEventoDominio>();
                foreach (var servico in servicos)
                {
                    eventosItens.Add(ordemCriada.AdicionarServicoNaAberturaComEvento(
                        servico.Id, servico.Nome, servico.Preco, servico.TempoEstimado));
                }

                foreach (var item in pecas)
                {
                    eventosItens.Add(ordemCriada.AdicionarPecaNaAberturaComEvento(
                        item.Peca.Id, item.Peca.Nome, item.Peca.Preco, item.Quantidade));
                }

                var ordemPersistida = await _ordemRepository.AtualizarAsync(ordemCriada, token);
                var eventoCriacao = ordemPersistida.CriarEventoOrdemCriada();
                await _historicoService.RegistrarAsync(
                    ordemPersistida,
                    eventoCriacao.TipoEvento,
                    eventoCriacao.StatusAnterior,
                    eventoCriacao.StatusNovo,
                    eventoCriacao.Descricao,
                    token);
                foreach (var eventoItem in eventosItens)
                {
                    await _historicoService.RegistrarAsync(
                        ordemPersistida,
                        eventoItem.TipoEvento,
                        eventoItem.StatusAnterior,
                        eventoItem.StatusNovo,
                        eventoItem.Descricao,
                        token);
                }

                await _notificacaoService.RegistrarAsync(
                    ordemPersistida.Id,
                    TipoNotificacaoCliente.LinkAcompanhamentoEnviado,
                    CanalNotificacaoCliente.Email,
                    $"Link de acompanhamento da ordem {ordemPersistida.Numero} enviado para o e-mail {cliente.Email}. Acesso requer autenticacao do cliente por CPF/CNPJ. Endpoint: {OrdemDeServicoAcompanhamentoService.MontarEndpointAcompanhamento(ordemPersistida.CodigoAcompanhamento)}",
                    token);

                return ordemPersistida;
            }, cancellationToken);
            _businessMetrics.RecordOrderCreated();
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Ordem de servico aberta com sucesso. Numero: {ordemAtualizada.Numero}");
            return OrdemDeServicoMapper.ToResult(ordemAtualizada);
        }
        catch (Exception ex) when (ex is IServiceExceptionContract serviceException)
        {
            _businessMetrics.RecordOrderCreationFailure(ClassifyBusinessFailure(serviceException.StatusCode));
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CriarOrdemDeServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _businessMetrics.RecordOrderCreationFailure("unexpected");
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CriarOrdemDeServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    private static string ClassifyBusinessFailure(int statusCode)
    {
        return statusCode switch
        {
            400 => "validation",
            404 => "not_found",
            _ => "business"
        };
    }
}
