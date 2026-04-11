using Microsoft.AspNetCore.Mvc;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ServicosController : ControllerBase
{
    private readonly IServicoApplicationService _servicoService;

    public ServicosController(IServicoApplicationService servicoService)
    {
        _servicoService = servicoService;
    }

    [HttpPost]
    public async Task<ActionResult<ServicoDto>> Criar([FromBody] CriarServicoDto dto, CancellationToken cancellationToken)
    {
        var servico = await _servicoService.CriarServicoAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = servico.Id }, servico);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServicoDto>> Obter(Guid id, CancellationToken cancellationToken)
    {
        var servico = await _servicoService.ObterServicoAsync(id, cancellationToken);
        return Ok(servico);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServicoDto>>> Listar(CancellationToken cancellationToken)
    {
        var servicos = await _servicoService.ListarServicosAsync(cancellationToken);
        return Ok(servicos);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ServicoDto>> Atualizar(Guid id, [FromBody] AtualizarServicoDto dto, CancellationToken cancellationToken)
    {
        var servico = await _servicoService.AtualizarServicoAsync(id, dto, cancellationToken);
        return Ok(servico);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id, CancellationToken cancellationToken)
    {
        await _servicoService.DeletarServicoAsync(id, cancellationToken);
        return NoContent();
    }
}
