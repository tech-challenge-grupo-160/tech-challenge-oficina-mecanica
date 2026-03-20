using Microsoft.AspNetCore.Mvc;
using oficina_mecanica.Application.DTOs;
using oficina_mecanica.Application.Services;

namespace oficina_mecanica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VeiculosController : ControllerBase
{
    private readonly IVeiculoApplicationService _veiculoService;

    public VeiculosController(IVeiculoApplicationService veiculoService)
    {
        _veiculoService = veiculoService;
    }

    [HttpPost]
    public async Task<ActionResult<VeiculoDto>> Criar([FromBody] CriarVeiculoDto dto)
    {
        var veiculo = await _veiculoService.CriarVeiculoAsync(dto);
        return CreatedAtAction(nameof(Obter), new { id = veiculo.Id }, veiculo);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VeiculoDto>> Obter(Guid id)
    {
        var veiculo = await _veiculoService.ObterVeiculoAsync(id);
        return Ok(veiculo);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VeiculoDto>>> Listar()
    {
        var veiculos = await _veiculoService.ListarVeiculosAsync();
        return Ok(veiculos);
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<IEnumerable<VeiculoDto>>> ListarPorCliente(Guid clienteId)
    {
        var veiculos = await _veiculoService.ListarVeiculosPorClienteAsync(clienteId);
        return Ok(veiculos);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VeiculoDto>> Atualizar(Guid id, [FromBody] AtualizarVeiculoDto dto)
    {
        var veiculo = await _veiculoService.AtualizarVeiculoAsync(id, dto);
        return Ok(veiculo);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        await _veiculoService.DeletarVeiculoAsync(id);
        return NoContent();
    }
}
