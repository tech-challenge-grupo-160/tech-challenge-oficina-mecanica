using Microsoft.AspNetCore.Mvc;
using oficina_mecanica.Application.DTOs;
using oficina_mecanica.Application.Services;

namespace oficina_mecanica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicosController : ControllerBase
{
    private readonly IServicoApplicationService _servicoService;

    public ServicosController(IServicoApplicationService servicoService)
    {
        _servicoService = servicoService;
    }

    [HttpPost]
    public async Task<ActionResult<ServicoDto>> Criar([FromBody] CriarServicoDto dto)
    {
        var servico = await _servicoService.CriarServicoAsync(dto);
        return CreatedAtAction(nameof(Obter), new { id = servico.Id }, servico);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServicoDto>> Obter(Guid id)
    {
        var servico = await _servicoService.ObterServicoAsync(id);
        return Ok(servico);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServicoDto>>> Listar()
    {
        var servicos = await _servicoService.ListarServicosAsync();
        return Ok(servicos);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ServicoDto>> Atualizar(Guid id, [FromBody] AtualizarServicoDto dto)
    {
        var servico = await _servicoService.AtualizarServicoAsync(id, dto);
        return Ok(servico);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        await _servicoService.DeletarServicoAsync(id);
        return NoContent();
    }
}
