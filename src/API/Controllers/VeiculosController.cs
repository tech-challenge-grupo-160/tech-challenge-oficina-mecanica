using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fiap.TechChallenge.OficinaMecanica.API.Mappers;
using Fiap.TechChallenge.OficinaMecanica.API.Requests.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Veiculos;
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
    public async Task<ActionResult<VeiculoResponse>> Criar([FromBody] CriarVeiculoRequest request, CancellationToken cancellationToken)
    {
        var veiculo = await _mediator.Send(request.ToCommand(), cancellationToken);
        var response = veiculo.ToResponse();
        return CreatedAtAction(nameof(ObterPorPlaca), new { placa = response.Placa }, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VeiculoResponse>> Obter(int id, CancellationToken cancellationToken)
    {
        var veiculo = await _mediator.Send(id.ToQueryById(), cancellationToken);
        return Ok(veiculo.ToResponse());
    }

    [HttpGet("placa/{placa}")]
    public async Task<ActionResult<VeiculoResponse>> ObterPorPlaca(string placa, CancellationToken cancellationToken)
    {
        var veiculo = await _mediator.Send(placa.ToQueryByPlaca(), cancellationToken);
        return Ok(veiculo.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VeiculoResponse>>> Listar(CancellationToken cancellationToken)
    {
        var veiculos = await _mediator.Send(new ListarVeiculosQuery(), cancellationToken);
        return Ok(veiculos.ToResponse());
    }

    [HttpGet("cliente/{clienteId:int}")]
    public async Task<ActionResult<IEnumerable<VeiculoResponse>>> ListarPorCliente(int clienteId, CancellationToken cancellationToken)
    {
        var veiculos = await _mediator.Send(new ListarVeiculosPorClienteQuery { ClienteId = clienteId }, cancellationToken);
        return Ok(veiculos.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VeiculoResponse>> Atualizar(int id, [FromBody] AtualizarVeiculoRequest request, CancellationToken cancellationToken)
    {
        var veiculo = await _mediator.Send(request.ToCommand(id), cancellationToken);
        return Ok(veiculo.ToResponse());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(id.ToDeleteCommand(), cancellationToken);
        return NoContent();
    }
}
