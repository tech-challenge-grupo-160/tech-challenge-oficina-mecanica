using Microsoft.AspNetCore.Mvc;
using oficina_mecanica.Application.DTOs;
using oficina_mecanica.Application.Services;

namespace oficina_mecanica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PecasController : ControllerBase
{
    private readonly IPecaApplicationService _pecaService;

    public PecasController(IPecaApplicationService pecaService)
    {
        _pecaService = pecaService;
    }

    [HttpPost]
    public async Task<ActionResult<PecaDto>> Criar([FromBody] CriarPecaDto dto)
    {
        var peca = await _pecaService.CriarPecaAsync(dto);
        return CreatedAtAction(nameof(Obter), new { id = peca.Id }, peca);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PecaDto>> Obter(Guid id)
    {
        var peca = await _pecaService.ObterPecaAsync(id);
        return Ok(peca);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PecaDto>>> Listar()
    {
        var pecas = await _pecaService.ListarPecasAsync();
        return Ok(pecas);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PecaDto>> Atualizar(Guid id, [FromBody] AtualizarPecaDto dto)
    {
        var peca = await _pecaService.AtualizarPecaAsync(id, dto);
        return Ok(peca);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        await _pecaService.DeletarPecaAsync(id);
        return NoContent();
    }
}
