using Fiap.TechChallenge.OficinaMecanica.API.Mappers;
using Fiap.TechChallenge.OficinaMecanica.API.Requests.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.API.Responses;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.PedidosCompra;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/pedidos-compra")]
public class PedidosCompraController : ControllerBase
{
    private readonly IMediator _mediator;

    public PedidosCompraController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<PedidoCompraResponse>> Criar([FromBody] CriarPedidoCompraRequest request, CancellationToken cancellationToken)
    {
        var pedido = await _mediator.Send(request.ToCommand(), cancellationToken);
        var response = pedido.ToResponse();
        return CreatedAtAction(nameof(ListarPorOrdemDeServico), new { ordemDeServicoId = response.OrdemDeServicoId }, response);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<PedidoCompraResponse>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var pedidos = await _mediator.Send(new ListarPedidosCompraQuery
        {
            Page = page,
            PageSize = pageSize
        }, cancellationToken);
        return Ok(pedidos.ToResponse());
    }

    [HttpGet("ordem/{ordemDeServicoId:int}")]
    public async Task<ActionResult<IEnumerable<PedidoCompraResponse>>> ListarPorOrdemDeServico(int ordemDeServicoId, CancellationToken cancellationToken)
    {
        var pedidos = await _mediator.Send(new ListarPedidosCompraPorOrdemQuery
        {
            OrdemDeServicoId = ordemDeServicoId
        }, cancellationToken);
        return Ok(pedidos.ToResponse());
    }

    [HttpPatch("{id:int}/receber")]
    public async Task<ActionResult<PedidoCompraResponse>> RegistrarRecebimento(int id, [FromBody] ReceberPedidoCompraRequest request, CancellationToken cancellationToken)
    {
        var pedido = await _mediator.Send(request.ToCommand(id), cancellationToken);
        return Ok(pedido.ToResponse());
    }
}
