using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class VeiculosController : ControllerBase
{
    private readonly IVeiculoApplicationService _veiculoService;

    public VeiculosController(IVeiculoApplicationService veiculoService)
    {
        _veiculoService = veiculoService;
    }

    [HttpPost]
    public async Task<ActionResult<VeiculoDto>> Criar([FromBody] CriarVeiculoDto dto, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoService.CriarVeiculoAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObterPorPlaca), new { placa = veiculo.Placa }, veiculo);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VeiculoDto>> Obter(int id, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoService.ObterVeiculoAsync(id, cancellationToken);
        return Ok(veiculo);
    }

    [HttpGet("placa/{placa}")]
    public async Task<ActionResult<VeiculoDto>> ObterPorPlaca(string placa, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoService.ObterVeiculoPorPlacaAsync(placa, cancellationToken);
        return Ok(veiculo);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VeiculoDto>>> Listar(CancellationToken cancellationToken)
    {
        var veiculos = await _veiculoService.ListarVeiculosAsync(cancellationToken);
        return Ok(veiculos);
    }

    [HttpGet("cliente/{clienteId:int}")]
    public async Task<ActionResult<IEnumerable<VeiculoDto>>> ListarPorCliente(int clienteId, CancellationToken cancellationToken)
    {
        var veiculos = await _veiculoService.ListarVeiculosPorClienteAsync(clienteId, cancellationToken);
        return Ok(veiculos);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VeiculoDto>> Atualizar(int id, [FromBody] AtualizarVeiculoDto dto, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoService.AtualizarVeiculoAsync(id, dto, cancellationToken);
        return Ok(veiculo);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id, CancellationToken cancellationToken)
    {
        await _veiculoService.DeletarVeiculoAsync(id, cancellationToken);
        return NoContent();
    }
}
