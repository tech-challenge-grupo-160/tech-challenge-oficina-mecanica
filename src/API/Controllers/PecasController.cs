using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class PecasController : ControllerBase
{
    private readonly IPecaApplicationService _pecaService;

    public PecasController(IPecaApplicationService pecaService)
    {
        _pecaService = pecaService;
    }

    [HttpPost]
    public async Task<ActionResult<PecaDto>> Criar([FromBody] CriarPecaDto dto, CancellationToken cancellationToken)
    {
        var peca = await _pecaService.CriarPecaAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = peca.Id }, peca);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PecaDto>> Obter(int id, CancellationToken cancellationToken)
    {
        var peca = await _pecaService.ObterPecaAsync(id, cancellationToken);
        return Ok(peca);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PecaDto>>> Listar(CancellationToken cancellationToken)
    {
        var pecas = await _pecaService.ListarPecasAsync(cancellationToken);
        return Ok(pecas);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PecaDto>> Atualizar(int id, [FromBody] AtualizarPecaDto dto, CancellationToken cancellationToken)
    {
        var peca = await _pecaService.AtualizarPecaAsync(id, dto, cancellationToken);
        return Ok(peca);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id, CancellationToken cancellationToken)
    {
        await _pecaService.DeletarPecaAsync(id, cancellationToken);
        return NoContent();
    }
}
