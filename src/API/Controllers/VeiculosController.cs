using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fiap.TechChallenge.OficinaMecanica.API.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class VeiculosController : ControllerBase
{
    private readonly IMediator _mediator;

    public VeiculosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<VeiculoDto>> Criar([FromBody] CriarVeiculoDto dto, CancellationToken cancellationToken)
    {
        var veiculo = await _mediator.Send(dto.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(ObterPorPlaca), new { placa = veiculo.Placa }, veiculo);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VeiculoDto>> Obter(int id, CancellationToken cancellationToken)
    {
        var veiculo = await _mediator.Send(id.ToQueryById(), cancellationToken);
        return Ok(veiculo);
    }

    [HttpGet("placa/{placa}")]
    public async Task<ActionResult<VeiculoDto>> ObterPorPlaca(string placa, CancellationToken cancellationToken)
    {
        var veiculo = await _mediator.Send(placa.ToQueryByPlaca(), cancellationToken);
        return Ok(veiculo);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VeiculoDto>>> Listar(CancellationToken cancellationToken)
    {
        var veiculos = await _mediator.Send(new ListarVeiculosQuery(), cancellationToken);
        return Ok(veiculos);
    }

    [HttpGet("cliente/{clienteId:int}")]
    public async Task<ActionResult<IEnumerable<VeiculoDto>>> ListarPorCliente(int clienteId, CancellationToken cancellationToken)
    {
        var veiculos = await _mediator.Send(new ListarVeiculosPorClienteQuery { ClienteId = clienteId }, cancellationToken);
        return Ok(veiculos);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VeiculoDto>> Atualizar(int id, [FromBody] AtualizarVeiculoDto dto, CancellationToken cancellationToken)
    {
        var veiculo = await _mediator.Send(dto.ToCommand(id), cancellationToken);
        return Ok(veiculo);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(id.ToDeleteCommand(), cancellationToken);
        return NoContent();
    }
}
