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
    private readonly OrdemDeServicoHistoricoService _historicoService;
    private readonly OrdemDeServicoNotificacaoService _notificacaoService;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly ILogger _logger;

    public CriarOrdemDeServicoCommandHandler(
        OrdemDeServicoAcompanhamentoService acompanhamentoService,
        IClienteRepository clienteRepository,
        IClock clock,
        OrdemDeServicoHistoricoService historicoService,
        OrdemDeServicoNotificacaoService notificacaoService,
        IOrdemDeServicoRepository ordemRepository,
        IPecaRepository pecaRepository,
        IServicoRepository servicoRepository,
        IVeiculoRepository veiculoRepository,
        ILoggerFactory loggerFactory)
    {
        _acompanhamentoService = acompanhamentoService;
        _clienteRepository = clienteRepository;
        _clock = clock;
        _historicoService = historicoService;
        _notificacaoService = notificacaoService;
        _ordemRepository = ordemRepository;
        _pecaRepository = pecaRepository;
        _servicoRepository = servicoRepository;
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

            var servicos = new List<Servico>();
            foreach (var item in command.Servicos)
            {
                var servico = await _servicoRepository.ObterPorIdAsync(item.ServicoId, cancellationToken);
                if (servico == null)
                {
                    _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Servico nao encontrado para abertura da OS");
                    throw new ServiceNotFoundException($"Servico com ID {item.ServicoId} nao encontrado.");
                }

                servicos.Add(servico);
            }

            var pecas = new List<(Peca Peca, int Quantidade)>();
            foreach (var item in command.Pecas)
            {
                var peca = await _pecaRepository.ObterPorIdAsync(item.PecaId, cancellationToken);
                if (peca == null)
                {
                    _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Peca nao encontrada para abertura da OS");
                    throw new ServiceNotFoundException($"Peca com ID {item.PecaId} nao encontrada.");
                }

                pecas.Add((peca, item.Quantidade));
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarOrdemDeServicoAsync), "Persistindo ordem de servico em status Recebida");
            var (codigoAcompanhamento, tokenAcompanhamento, tokenAcompanhamentoHash) =
                await _acompanhamentoService.GerarCredenciaisAsync(cancellationToken);

            var ordem = OrdemDeServico.Criar(
                OrdemDeServicoAcompanhamentoService.GerarNumeroTemporario(),
                codigoAcompanhamento,
                tokenAcompanhamentoHash,
                command.ClienteId,
                command.VeiculoId,
                command.DescricaoSolicitacao,
                command.ObservacoesRecepcao,
                _clock.Now);

            var ordemCriada = await _ordemRepository.CriarAsync(ordem, cancellationToken);
            ordemCriada.DefinirNumero(OrdemDeServicoAcompanhamentoService.GerarNumeroOrdem(ordemCriada.Id, ordemCriada.DataAbertura));

            var eventosItens = new List<OrdemDeServicoEventoDominio>();
            foreach (var servico in servicos)
            {
                eventosItens.Add(ordemCriada.AdicionarServicoNaAberturaComEvento(servico));
            }

            foreach (var item in pecas)
            {
                eventosItens.Add(ordemCriada.AdicionarPecaNaAberturaComEvento(item.Peca, item.Quantidade));
            }

            var ordemAtualizada = await _ordemRepository.AtualizarAsync(ordemCriada, cancellationToken);
            var eventoCriacao = ordemAtualizada.CriarEventoOrdemCriada();
            await _historicoService.RegistrarAsync(
                ordemAtualizada,
                eventoCriacao.TipoEvento,
                eventoCriacao.StatusAnterior,
                eventoCriacao.StatusNovo,
                eventoCriacao.Descricao,
                cancellationToken);
            foreach (var eventoItem in eventosItens)
            {
                await _historicoService.RegistrarAsync(
                    ordemAtualizada,
                    eventoItem.TipoEvento,
                    eventoItem.StatusAnterior,
                    eventoItem.StatusNovo,
                    eventoItem.Descricao,
                    cancellationToken);
            }

            await _notificacaoService.RegistrarAsync(
                ordemAtualizada.Id,
                TipoNotificacaoCliente.LinkAcompanhamentoEnviado,
                CanalNotificacaoCliente.Email,
                $"Link de acompanhamento da ordem {ordemAtualizada.Numero} enviado para o e-mail {cliente.Email}. Endpoint: {OrdemDeServicoAcompanhamentoService.MontarEndpointAcompanhamento(ordemAtualizada.CodigoAcompanhamento)}",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Ordem de servico aberta com sucesso. Numero: {ordemAtualizada.Numero}");
            var resposta = OrdemDeServicoMapper.ToResult(ordemAtualizada);
            resposta.TokenAcompanhamento = tokenAcompanhamento;
            return resposta;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(CriarOrdemDeServicoAsync), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}


