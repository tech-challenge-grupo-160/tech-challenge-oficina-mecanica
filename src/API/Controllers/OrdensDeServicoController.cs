using Microsoft.AspNetCore.Mvc;
using oficina_mecanica.Application.DTOs;
using oficina_mecanica.Application.Services;

namespace oficina_mecanica.API.Controllers;

[ApiController]
[Route("api/ordens-servico")]
public class OrdensDeServicoController : ControllerBase
{
    private readonly IOrdemDeServicoApplicationService _ordemService;

    public OrdensDeServicoController(IOrdemDeServicoApplicationService ordemService)
    {
        _ordemService = ordemService;
    }

    [HttpPost]
    public async Task<ActionResult<OrdemDeServicoDto>> Criar([FromBody] CriarOrdemDeServicoDto dto)
    {
        var ordem = await _ordemService.CriarOrdemDeServicoAsync(dto);
        return CreatedAtAction(nameof(Obter), new { id = ordem.Id }, ordem);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrdemDeServicoDto>> Obter(Guid id)
    {
        var ordem = await _ordemService.ObterOrdemDeServicoAsync(id);
        return Ok(ordem);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrdemDeServicoDto>>> Listar()
    {
        var ordens = await _ordemService.ListarOrdensDeServicoAsync();
        return Ok(ordens);
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<IEnumerable<OrdemDeServicoDto>>> ListarPorCliente(Guid clienteId)
    {
        var ordens = await _ordemService.ListarOrdensDeServicoPorClienteAsync(clienteId);
        return Ok(ordens);
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<OrdemDeServicoDto>>> ListarPorStatus(string status)
    {
        var ordens = await _ordemService.ListarOrdensDeServicoPorStatusAsync(status);
        return Ok(ordens);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<OrdemDeServicoDto>> AtualizarStatus(Guid id, [FromBody] AtualizarStatusOrdemDeServicoDto dto)
    {
        var ordem = await _ordemService.AtualizarStatusAsync(id, dto);
        return Ok(ordem);
    }

    [HttpPost("{id}/servicos")]
    public async Task<ActionResult<OrdemDeServicoDto>> AdicionarServico(Guid id, [FromBody] AdicionarServicoAOrdemDto dto)
    {
        var ordem = await _ordemService.AdicionarServicoAsync(id, dto);
        return Ok(ordem);
    }

    [HttpPost("{id}/pecas")]
    public async Task<ActionResult<OrdemDeServicoDto>> AdicionarPeca(Guid id, [FromBody] AdicionarPecaAOrdemDto dto)
    {
        var ordem = await _ordemService.AdicionarPecaAsync(id, dto);
        return Ok(ordem);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        await _ordemService.DeletarOrdemDeServicoAsync(id);
        return NoContent();
    }
}
