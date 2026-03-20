using Microsoft.AspNetCore.Mvc;
using oficina_mecanica.Application.DTOs;
using oficina_mecanica.Application.Services;

namespace oficina_mecanica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteApplicationService _clienteService;

    public ClientesController(IClienteApplicationService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Criar([FromBody] CriarClienteDto dto)
    {
        var cliente = await _clienteService.CriarClienteAsync(dto);
        return CreatedAtAction(nameof(Obter), new { id = cliente.Id }, cliente);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteDto>> Obter(Guid id)
    {
        var cliente = await _clienteService.ObterClienteAsync(id);
        return Ok(cliente);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> Listar()
    {
        var clientes = await _clienteService.ListarClientesAsync();
        return Ok(clientes);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClienteDto>> Atualizar(Guid id, [FromBody] AtualizarClienteDto dto)
    {
        var cliente = await _clienteService.AtualizarClienteAsync(id, dto);
        return Ok(cliente);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        await _clienteService.DeletarClienteAsync(id);
        return NoContent();
    }
}
