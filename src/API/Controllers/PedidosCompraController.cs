using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/pedidos-compra")]
public class PedidosCompraController : ControllerBase
{
    private readonly IPedidoCompraApplicationService _pedidoCompraService;

    public PedidosCompraController(IPedidoCompraApplicationService pedidoCompraService)
    {
        _pedidoCompraService = pedidoCompraService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<PedidoCompraDto>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0)
        {
            return BadRequest("page deve ser maior que zero.");
        }

        if (pageSize <= 0)
        {
            return BadRequest("pageSize deve ser maior que zero.");
        }

        var pedidos = await _pedidoCompraService.ListarAsync(page, pageSize, cancellationToken);
        return Ok(pedidos);
    }

    [HttpGet("ordem/{ordemDeServicoId:int}")]
    public async Task<ActionResult<IEnumerable<PedidoCompraDto>>> ListarPorOrdemDeServico(int ordemDeServicoId, CancellationToken cancellationToken)
    {
        var pedidos = await _pedidoCompraService.ListarPorOrdemDeServicoAsync(ordemDeServicoId, cancellationToken);
        return Ok(pedidos);
    }

    [HttpPatch("{id:int}/receber")]
    public async Task<ActionResult<PedidoCompraDto>> RegistrarRecebimento(int id, [FromBody] ReceberPedidoCompraDto dto, CancellationToken cancellationToken)
    {
        var pedido = await _pedidoCompraService.RegistrarRecebimentoAsync(id, dto, cancellationToken);
        return Ok(pedido);
    }
}
