using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/ordens-servico")]
public class OrdensDeServicoController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdensDeServicoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<OrdemDeServicoDto>> Criar([FromBody] CriarOrdemDeServicoDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new CriarOrdemDeServicoCommand
        {
            ClienteId = dto.ClienteId,
            VeiculoId = dto.VeiculoId,
            DescricaoSolicitacao = dto.DescricaoSolicitacao,
            ObservacoesRecepcao = dto.ObservacoesRecepcao
        }, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = ordem.Id }, ordem);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrdemDeServicoDto>> Obter(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new ObterOrdemDeServicoPorIdQuery { Id = id }, cancellationToken);
        return Ok(ordem);
    }

    [HttpGet("{id}/historico")]
    public async Task<ActionResult<IEnumerable<OrdemServicoHistoricoDto>>> ObterHistorico(int id, CancellationToken cancellationToken)
    {
        var historico = await _mediator.Send(new ObterHistoricoOrdemDeServicoQuery { Id = id }, cancellationToken);
        return Ok(historico);
    }

    [HttpGet("{id}/notificacoes")]
    public async Task<ActionResult<IEnumerable<NotificacaoClienteDto>>> ObterNotificacoes(int id, CancellationToken cancellationToken)
    {
        var notificacoes = await _mediator.Send(new ObterNotificacoesOrdemDeServicoQuery { Id = id }, cancellationToken);
        return Ok(notificacoes);
    }

    [HttpGet("{id}/movimentacoes-estoque")]
    public async Task<ActionResult<IEnumerable<MovimentacoesEstoquePorPecaDto>>> ObterMovimentacoesEstoque(int id, CancellationToken cancellationToken)
    {
        var movimentacoes = await _mediator.Send(new ObterMovimentacoesEstoqueOrdemDeServicoQuery { Id = id }, cancellationToken);
        return Ok(movimentacoes);
    }

    [HttpGet("monitoramento")]
    public async Task<ActionResult<ResumoMonitoramentoOrdensDeServicoDto>> ObterResumoMonitoramento(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var resumo = await _mediator.Send(new ObterResumoMonitoramentoOrdensDeServicoQuery
        {
            Page = page,
            PageSize = pageSize
        }, cancellationToken);
        return Ok(resumo);
    }

    [HttpGet("{id}/monitoramento")]
    public async Task<ActionResult<MonitoramentoOrdemDeServicoDto>> ObterMonitoramento(int id, CancellationToken cancellationToken)
    {
        var monitoramento = await _mediator.Send(new ObterMonitoramentoOrdemDeServicoQuery { Id = id }, cancellationToken);
        return Ok(monitoramento);
    }

    [HttpGet("{id}/estimativa-tempo-servico")]
    public async Task<ActionResult<EstimativaTempoOrdemDeServicoDto>> ObterEstimativaTempo(int id, CancellationToken cancellationToken)
    {
        var estimativa = await _mediator.Send(new ObterEstimativaTempoOrdemDeServicoQuery { Id = id }, cancellationToken);
        return Ok(estimativa);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<OrdemDeServicoDto>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? clienteId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? numero = null,
        [FromQuery] DateTime? dataAberturaInicio = null,
        [FromQuery] DateTime? dataAberturaFim = null,
        CancellationToken cancellationToken = default)
    {
        var ordens = await _mediator.Send(new ListarOrdensDeServicoQuery
        {
            Page = page,
            PageSize = pageSize,
            ClienteId = clienteId,
            Status = status,
            Numero = numero,
            DataAberturaInicio = dataAberturaInicio,
            DataAberturaFim = dataAberturaFim
        }, cancellationToken);
        return Ok(ordens);
    }

    [HttpPatch("{id}/iniciar-diagnostico")]
    public async Task<ActionResult<OrdemDeServicoDto>> IniciarDiagnostico(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new IniciarDiagnosticoCommand { Id = id }, cancellationToken);
        return Ok(ordem);
    }

    [HttpPatch("{id}/finalizar-diagnostico")]
    public async Task<ActionResult<OrdemDeServicoDto>> FinalizarDiagnostico(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new FinalizarDiagnosticoCommand { Id = id }, cancellationToken);
        return Ok(ordem);
    }

    [HttpPatch("{id}/aprovar")]
    public async Task<ActionResult<OrdemDeServicoDto>> Aprovar(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new AprovarOrdemDeServicoCommand { Id = id }, cancellationToken);
        return Ok(ordem);
    }

    [HttpPatch("{id}/liberar-execucao")]
    public async Task<ActionResult<OrdemDeServicoDto>> LiberarExecucao(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new LiberarExecucaoCommand { Id = id }, cancellationToken);
        return Ok(ordem);
    }

    [HttpPatch("{id}/finalizar")]
    public async Task<ActionResult<OrdemDeServicoDto>> Finalizar(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new FinalizarOrdemDeServicoCommand { Id = id }, cancellationToken);
        return Ok(ordem);
    }

    [HttpPatch("{id}/registrar-pagamento")]
    public async Task<ActionResult<OrdemDeServicoDto>> RegistrarPagamento(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new RegistrarPagamentoCommand { Id = id }, cancellationToken);
        return Ok(ordem);
    }

    [HttpPatch("{id}/entregar")]
    public async Task<ActionResult<OrdemDeServicoDto>> Entregar(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new EntregarOrdemDeServicoCommand { Id = id }, cancellationToken);
        return Ok(ordem);
    }

    [HttpPatch("{id}/cancelar")]
    public async Task<ActionResult<OrdemDeServicoDto>> Cancelar(int id, [FromBody] CancelarOrdemDeServicoDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new CancelarOrdemDeServicoCommand
        {
            Id = id,
            MotivoCancelamento = dto.MotivoCancelamento
        }, cancellationToken);
        return Ok(ordem);
    }

    [HttpPost("{id}/servicos")]
    public async Task<ActionResult<OrdemDeServicoDto>> AdicionarServico(int id, [FromBody] AdicionarServicoAOrdemDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new AdicionarServicoAOrdemCommand
        {
            OrdemDeServicoId = id,
            ServicoId = dto.ServicoId
        }, cancellationToken);
        return Ok(ordem);
    }

    [HttpDelete("{id}/servicos/{servicoId:int}")]
    public async Task<ActionResult<OrdemDeServicoDto>> RemoverServico(int id, int servicoId, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new RemoverServicoDaOrdemCommand
        {
            OrdemDeServicoId = id,
            ServicoId = servicoId
        }, cancellationToken);
        return Ok(ordem);
    }

    [HttpPost("{id}/pecas")]
    public async Task<ActionResult<OrdemDeServicoDto>> AdicionarPeca(int id, [FromBody] AdicionarPecaAOrdemDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new AdicionarPecaAOrdemCommand
        {
            OrdemDeServicoId = id,
            PecaId = dto.PecaId,
            Quantidade = dto.Quantidade
        }, cancellationToken);
        return Ok(ordem);
    }

    [HttpDelete("{id}/pecas/{pecaId:int}")]
    public async Task<ActionResult<OrdemDeServicoDto>> RemoverPeca(int id, int pecaId, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new RemoverPecaDaOrdemCommand
        {
            OrdemDeServicoId = id,
            PecaId = pecaId
        }, cancellationToken);
        return Ok(ordem);
    }

}
