using Microsoft.AspNetCore.Mvc;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/v1/ordens-servico")]
public class OrdensDeServicoController : ControllerBase
{
    private readonly IOrdemDeServicoApplicationService _ordemService;

    public OrdensDeServicoController(IOrdemDeServicoApplicationService ordemService)
    {
        _ordemService = ordemService;
    }

    [HttpPost]
    public async Task<ActionResult<OrdemDeServicoDto>> Criar([FromBody] CriarOrdemDeServicoDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _ordemService.CriarOrdemDeServicoAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = ordem.Id }, ordem);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrdemDeServicoDto>> Obter(Guid id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemService.ObterOrdemDeServicoAsync(id, cancellationToken);
        return Ok(ordem);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrdemDeServicoDto>>> Listar(CancellationToken cancellationToken)
    {
        var ordens = await _ordemService.ListarOrdensDeServicoAsync(cancellationToken);
        return Ok(ordens);
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<IEnumerable<OrdemDeServicoDto>>> ListarPorCliente(Guid clienteId, CancellationToken cancellationToken)
    {
        var ordens = await _ordemService.ListarOrdensDeServicoPorClienteAsync(clienteId, cancellationToken);
        return Ok(ordens);
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<OrdemDeServicoDto>>> ListarPorStatus(string status, CancellationToken cancellationToken)
    {
        var ordens = await _ordemService.ListarOrdensDeServicoPorStatusAsync(status, cancellationToken);
        return Ok(ordens);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<OrdemDeServicoDto>> AtualizarStatus(Guid id, [FromBody] AtualizarStatusOrdemDeServicoDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _ordemService.AtualizarStatusAsync(id, dto, cancellationToken);
        return Ok(ordem);
    }

    [HttpPost("{id}/servicos")]
    public async Task<ActionResult<OrdemDeServicoDto>> AdicionarServico(Guid id, [FromBody] AdicionarServicoAOrdemDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _ordemService.AdicionarServicoAsync(id, dto, cancellationToken);
        return Ok(ordem);
    }

    [HttpPost("{id}/pecas")]
    public async Task<ActionResult<OrdemDeServicoDto>> AdicionarPeca(Guid id, [FromBody] AdicionarPecaAOrdemDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _ordemService.AdicionarPecaAsync(id, dto, cancellationToken);
        return Ok(ordem);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id, CancellationToken cancellationToken)
    {
        await _ordemService.DeletarOrdemDeServicoAsync(id, cancellationToken);
        return NoContent();
    }
}
