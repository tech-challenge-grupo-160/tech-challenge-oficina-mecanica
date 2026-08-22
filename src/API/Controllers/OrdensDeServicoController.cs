using Fiap.TechChallenge.OficinaMecanica.API.Authorization;
using Fiap.TechChallenge.OficinaMecanica.API.Mappers;
using Fiap.TechChallenge.OficinaMecanica.API.Requests.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.API.Responses;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

    [HttpGet("{id}/movimentacoes-estoque")]
    public async Task<ActionResult<IEnumerable<MovimentacoesEstoquePorPecaResponse>>> ObterMovimentacoesEstoque(
        int id,
        CancellationToken cancellationToken)
    {
        var movimentacoes = await _mediator.Send(id.ToMovimentacoesEstoqueOrdemDeServicoQuery(), cancellationToken);
        return Ok(movimentacoes.ToResponse());
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
        return Ok(ordem.ToResponse());
    }

    [HttpPatch("{id}/finalizar-diagnostico")]
    public async Task<ActionResult<OrdemDeServicoResponse>> FinalizarDiagnostico(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(id.ToFinalizarDiagnosticoCommand(), cancellationToken);
        return Ok(ordem.ToResponse());
    }

    [HttpPatch("{id}/aprovar")]
    public async Task<ActionResult<OrdemDeServicoResponse>> Aprovar(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(id.ToAprovarOrdemDeServicoCommand(), cancellationToken);
        return Ok(ordem.ToResponse());
    }

    [Authorize(Policy = ApiAuthorizationPolicies.Cliente)]
    [EnableRateLimiting("public")]
    [HttpPost("{id}/ordem/resposta")]
    public async Task<ActionResult<OrdemDeServicoResponse>> ResponderOrdem(
        int id,
        [FromBody] ResponderOrdemDeServicoRequest request,
        CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(request.ToCommand(id), cancellationToken);
        return Ok(ordem.ToResponse());
    }

    [HttpPatch("{id}/liberar-execucao")]
    public async Task<ActionResult<OrdemDeServicoResponse>> LiberarExecucao(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(id.ToLiberarExecucaoCommand(), cancellationToken);
        return Ok(ordem.ToResponse());
    }

    [HttpPatch("{id}/finalizar")]
    public async Task<ActionResult<OrdemDeServicoResponse>> Finalizar(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(id.ToFinalizarOrdemDeServicoCommand(), cancellationToken);
        return Ok(ordem.ToResponse());
    }

    [HttpPatch("{id}/registrar-pagamento")]
    public async Task<ActionResult<OrdemDeServicoResponse>> RegistrarPagamento(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(id.ToRegistrarPagamentoCommand(), cancellationToken);
        return Ok(ordem.ToResponse());
    }

    [HttpPatch("{id}/entregar")]
    public async Task<ActionResult<OrdemDeServicoResponse>> Entregar(int id, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(id.ToEntregarOrdemDeServicoCommand(), cancellationToken);
        return Ok(ordem.ToResponse());
    }

    [HttpPatch("{numero}/avancar-status")]
    public async Task<ActionResult<OrdemDeServicoResponse>> AvancarStatus(string numero, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(numero.ToAvancarStatusOrdemDeServicoCommand(), cancellationToken);
        return Ok(ordem.ToResponse());
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

    [HttpPost("{id}/servicos")]
    public async Task<ActionResult<OrdemDeServicoResponse>> AdicionarServico(
        int id,
        [FromBody] AdicionarServicoAOrdemRequest request,
        CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(request.ToCommand(id), cancellationToken);
        return Ok(ordem.ToResponse());
    }

    [HttpDelete("{id}/servicos/{servicoId:int}")]
    public async Task<ActionResult<OrdemDeServicoResponse>> RemoverServico(int id, int servicoId, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(id.ToRemoverServicoDaOrdemCommand(servicoId), cancellationToken);
        return Ok(ordem.ToResponse());
    }

    [HttpPost("{id}/pecas")]
    public async Task<ActionResult<OrdemDeServicoResponse>> AdicionarPeca(
        int id,
        [FromBody] AdicionarPecaAOrdemRequest request,
        CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(request.ToCommand(id), cancellationToken);
        return Ok(ordem.ToResponse());
    }

    [HttpDelete("{id}/pecas/{pecaId:int}")]
    public async Task<ActionResult<OrdemDeServicoResponse>> RemoverPeca(int id, int pecaId, CancellationToken cancellationToken)
    {
        var ordem = await _mediator.Send(id.ToRemoverPecaDaOrdemCommand(pecaId), cancellationToken);
        return Ok(ordem.ToResponse());
    }
}
