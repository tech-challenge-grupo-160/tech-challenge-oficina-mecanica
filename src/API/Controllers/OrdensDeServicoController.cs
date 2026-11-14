using Fiap.TechChallenge.OficinaMecanica.API.Mappers;
using Fiap.TechChallenge.OficinaMecanica.API.Requests.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.API.Responses;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<OrdemDeServicoResponse>> Criar(
        [FromBody] CriarOrdemDeServicoRequest request,
        CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(request.ToCommand(), cancellationToken);
        var response = ordem.ToResponse();
        return CreatedAtAction(nameof(Obter), new { id = response.Id }, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrdemDeServicoResponse>> Obter(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(id.ToOrdemDeServicoQueryById(), cancellationToken);
        return Ok(ordem.ToResponse());
    }

    [HttpGet("{id}/historico")]
    public async Task<ActionResult<IEnumerable<OrdemServicoHistoricoResponse>>> ObterHistorico(int id, CancellationToken cancellationToken)
    {
        var historico = await _mediator.Send(id.ToHistoricoOrdemDeServicoQuery(), cancellationToken);
        return Ok(historico.ToResponse());
    }

    [HttpGet("{id}/notificacoes")]
    public async Task<ActionResult<IEnumerable<NotificacaoClienteResponse>>> ObterNotificacoes(int id, CancellationToken cancellationToken)
    {
        var notificacoes = await _mediator.Send(id.ToNotificacoesOrdemDeServicoQuery(), cancellationToken);
        return Ok(notificacoes.ToResponse());
    }

    [HttpGet("monitoramento")]
    public async Task<ActionResult<ResumoMonitoramentoOrdensDeServicoResponse>> ObterResumoMonitoramento(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var resumo = await _mediator.Send(new ObterResumoMonitoramentoOrdensDeServicoQuery
        {
            Page = page,
            PageSize = pageSize
        }, cancellationToken);
        return Ok(resumo.ToResponse());
    }

    [HttpGet("{id}/monitoramento")]
    public async Task<ActionResult<MonitoramentoOrdemDeServicoResponse>> ObterMonitoramento(int id, CancellationToken cancellationToken)
    {
        var monitoramento = await _mediator.Send(id.ToMonitoramentoOrdemDeServicoQuery(), cancellationToken);
        return Ok(monitoramento.ToResponse());
    }

    [HttpGet("{id}/estimativa-tempo-servico")]
    public async Task<ActionResult<EstimativaTempoOrdemDeServicoResponse>> ObterEstimativaTempo(int id, CancellationToken cancellationToken)
    {
        var estimativa = await _mediator.Send(id.ToEstimativaTempoOrdemDeServicoQuery(), cancellationToken);
        return Ok(estimativa.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<OrdemDeServicoResponse>>> Listar(
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
        return Ok(ordens.ToResponse());
    }

    [HttpPatch("{id}/iniciar-diagnostico")]
    public async Task<ActionResult<OrdemDeServicoResponse>> IniciarDiagnostico(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(id.ToIniciarDiagnosticoCommand(), cancellationToken);
        return AcceptedAtAction(nameof(Obter), new { id }, ordem.ToResponse());
    }

    [HttpPatch("{id}/entregar")]
    public async Task<ActionResult<OrdemDeServicoResponse>> Entregar(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(id.ToEntregarOrdemDeServicoCommand(), cancellationToken);
        return Ok(ordem.ToResponse());
    }

    /// <summary>
    /// Nova cobranca depois de um pagamento recusado (F4-27). A OS segue
    /// Finalizada; o Billing cria o pagamento da proxima tentativa.
    /// </summary>
    [HttpPost("{id}/pagamento/nova-tentativa")]
    public async Task<ActionResult<OrdemDeServicoResponse>> SolicitarNovaCobranca(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(new SolicitarNovaCobrancaCommand { Id = id }, cancellationToken);
        return AcceptedAtAction(nameof(Obter), new { id }, ordem.ToResponse());
    }

    [HttpPatch("{id}/cancelar")]
    public async Task<ActionResult<OrdemDeServicoResponse>> Cancelar(
        int id,
        [FromBody] CancelarOrdemDeServicoRequest request,
        CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(request.ToCommand(id), cancellationToken);
        return Ok(ordem.ToResponse());
    }

}
