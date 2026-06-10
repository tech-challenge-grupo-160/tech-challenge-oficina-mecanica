using Fiap.TechChallenge.OficinaMecanica.API.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
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
    public async Task<ActionResult<PedidoCompraDto>> Criar([FromBody] CriarPedidoCompraDto dto, CancellationToken cancellationToken)
    {
        var pedido = await _mediator.Send(dto.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(ListarPorOrdemDeServico), new { ordemDeServicoId = pedido.OrdemDeServicoId }, pedido);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<PedidoCompraDto>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var pedidos = await _mediator.Send(new ListarPedidosCompraQuery
        {
            Page = page,
            PageSize = pageSize
        }, cancellationToken);
        return Ok(pedidos);
    }

    [HttpGet("ordem/{ordemDeServicoId:int}")]
    public async Task<ActionResult<IEnumerable<PedidoCompraDto>>> ListarPorOrdemDeServico(int ordemDeServicoId, CancellationToken cancellationToken)
    {
        var pedidos = await _mediator.Send(new ListarPedidosCompraPorOrdemQuery
        {
            OrdemDeServicoId = ordemDeServicoId
        }, cancellationToken);
        return Ok(pedidos);
    }

    [HttpPatch("{id:int}/receber")]
    public async Task<ActionResult<PedidoCompraDto>> RegistrarRecebimento(int id, [FromBody] ReceberPedidoCompraDto dto, CancellationToken cancellationToken)
    {
        var pedido = await _mediator.Send(dto.ToCommand(id), cancellationToken);
        return Ok(pedido);
    }
}
