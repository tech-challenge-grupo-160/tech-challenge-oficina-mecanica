using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fiap.TechChallenge.OficinaMecanica.API.Mappers;
using Fiap.TechChallenge.OficinaMecanica.API.Requests.Servicos;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Servicos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class ServicosController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServicosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ServicoResponse>> Criar([FromBody] CriarServicoRequest request, CancellationToken cancellationToken)
    {
        var servico = await _mediator.Send(request.ToCommand(), cancellationToken);
        var response = servico.ToResponse();
        return CreatedAtAction(nameof(Obter), new { id = response.Id }, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServicoResponse>> Obter(int id, CancellationToken cancellationToken)
    {
        var servico = await _mediator.Send(id.ToServicoQueryById(), cancellationToken);
        return Ok(servico.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServicoResponse>>> Listar(CancellationToken cancellationToken)
    {
        var servicos = await _mediator.Send(new ListarServicosQuery(), cancellationToken);
        return Ok(servicos.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ServicoResponse>> Atualizar(int id, [FromBody] AtualizarServicoRequest request, CancellationToken cancellationToken)
    {
        var servico = await _mediator.Send(request.ToCommand(id), cancellationToken);
        return Ok(servico.ToResponse());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(id.ToDeleteServicoCommand(), cancellationToken);
        return NoContent();
    }
}
