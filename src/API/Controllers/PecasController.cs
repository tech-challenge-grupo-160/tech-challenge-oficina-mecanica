using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fiap.TechChallenge.OficinaMecanica.API.Mappers;
using Fiap.TechChallenge.OficinaMecanica.API.Requests.Pecas;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Pecas;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class PecasController : ControllerBase
{
    private readonly IMediator _mediator;

    public PecasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<PecaResponse>> Criar([FromBody] CriarPecaRequest request, CancellationToken cancellationToken)
    {
        var peca = await _mediator.Send(request.ToCommand(), cancellationToken);
        var response = peca.ToResponse();
        return CreatedAtAction(nameof(Obter), new { id = response.Id }, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PecaResponse>> Obter(int id, CancellationToken cancellationToken)
    {
        var peca = await _mediator.Send(id.ToPecaQueryById(), cancellationToken);
        return Ok(peca.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PecaResponse>>> Listar(CancellationToken cancellationToken)
    {
        var pecas = await _mediator.Send(new ListarPecasQuery(), cancellationToken);
        return Ok(pecas.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PecaResponse>> Atualizar(int id, [FromBody] AtualizarPecaRequest request, CancellationToken cancellationToken)
    {
        var peca = await _mediator.Send(request.ToCommand(id), cancellationToken);
        return Ok(peca.ToResponse());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(id.ToDeletePecaCommand(), cancellationToken);
        return NoContent();
    }
}
