using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
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

public sealed class CriarOrdemDeServicoCommandHandler : IRequestHandler<CriarOrdemDeServicoCommand, OrdemDeServicoDto>
{
    private const string LoggerName = nameof(CriarOrdemDeServicoCommandHandler);
    private readonly OrdemDeServicoHandlerDependencies _dependencies;
    private readonly ILogger _logger;

    public CriarOrdemDeServicoCommandHandler(OrdemDeServicoHandlerDependencies dependencies)
    {
        _dependencies = dependencies;
        _logger = dependencies.LoggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoDto> Handle(CriarOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        return CriarOrdemDeServicoAsync(new CriarOrdemDeServicoDto { ClienteId = command.ClienteId, VeiculoId = command.VeiculoId, DescricaoSolicitacao = command.DescricaoSolicitacao, ObservacoesRecepcao = command.ObservacoesRecepcao }, cancellationToken);
    }

private async Task<OrdemDeServicoDto> CriarOrdemDeServicoAsync(CriarOrdemDeServicoDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarOrdemDeServicoAsync), "Consultando cliente e veiculo informados");
            var cliente = await _dependencies.ClienteRepository.ObterPorIdAsync(dto.ClienteId, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Cliente nao encontrado para abertura da OS");
                throw new ServiceNotFoundException($"Cliente com ID {dto.ClienteId} nao encontrado.");
            }

            var veiculo = await _dependencies.VeiculoRepository.ObterPorIdAsync(dto.VeiculoId, cancellationToken);
            if (veiculo == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Veiculo nao encontrado para abertura da OS");
                throw new ServiceNotFoundException($"Veiculo com ID {dto.VeiculoId} nao encontrado.");
            }

            if (veiculo.ClienteId != dto.ClienteId)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Veiculo nao pertence ao cliente informado");
                throw new ServiceValidationException("O veiculo informado nao pertence ao cliente informado.");
            }

            var existeOrdemAtiva = await _dependencies.OrdemRepository.ExisteOrdemAtivaPorClienteEVeiculoAsync(dto.ClienteId, dto.VeiculoId, cancellationToken);
            if (existeOrdemAtiva)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(CriarOrdemDeServicoAsync), "Ja existe ordem de servico ativa para o cliente e veiculo informados");
                throw new ServiceValidationException("Ja existe uma ordem de servico ativa para este cliente e veiculo.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(CriarOrdemDeServicoAsync), "Persistindo ordem de servico em status Recebida");
            var (codigoAcompanhamento, tokenAcompanhamento, tokenAcompanhamentoHash) =
                await _dependencies.AcompanhamentoService.GerarCredenciaisAsync(cancellationToken);

            var ordem = OrdemDeServico.Criar(
                OrdemDeServicoAcompanhamentoService.GerarNumeroTemporario(),
                codigoAcompanhamento,
                tokenAcompanhamentoHash,
                dto.ClienteId,
                dto.VeiculoId,
                dto.DescricaoSolicitacao,
                dto.ObservacoesRecepcao,
                _dependencies.Clock.Now);

            var ordemCriada = await _dependencies.OrdemRepository.CriarAsync(ordem, cancellationToken);
            ordemCriada.DefinirNumero(OrdemDeServicoAcompanhamentoService.GerarNumeroOrdem(ordemCriada.Id, ordemCriada.DataAbertura));

            var ordemAtualizada = await _dependencies.OrdemRepository.AtualizarAsync(ordemCriada, cancellationToken);
            var eventoCriacao = ordemAtualizada.CriarEventoOrdemCriada();
            await _dependencies.HistoricoService.RegistrarAsync(
                ordemAtualizada,
                eventoCriacao.TipoEvento,
                eventoCriacao.StatusAnterior,
                eventoCriacao.StatusNovo,
                eventoCriacao.Descricao,
                cancellationToken);
            await _dependencies.NotificacaoService.RegistrarAsync(
                ordemAtualizada.Id,
                TipoNotificacaoCliente.LinkAcompanhamentoEnviado,
                CanalNotificacaoCliente.Email,
                $"Link de acompanhamento da ordem {ordemAtualizada.Numero} enviado para o e-mail {cliente.Email}. Endpoint: {OrdemDeServicoAcompanhamentoService.MontarEndpointAcompanhamento(ordemAtualizada.CodigoAcompanhamento)}",
                cancellationToken);
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Ordem de servico aberta com sucesso. Numero: {ordemAtualizada.Numero}");
            var resposta = OrdemDeServicoMapper.ToDto(ordemAtualizada);
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
