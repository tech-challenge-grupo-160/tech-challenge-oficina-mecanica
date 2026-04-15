using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteApplicationService _clienteService;

    public ClientesController(IClienteApplicationService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Criar([FromBody] CriarClienteDto dto, CancellationToken cancellationToken)
    {
        var cliente = await _clienteService.CriarClienteAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObterPorDocumento), new { cpfCnpj = cliente.CpfCnpj }, cliente);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteDto>> Obter(int id, CancellationToken cancellationToken)
    {
        var cliente = await _clienteService.ObterClienteAsync(id, cancellationToken);
        return Ok(cliente);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> Listar(CancellationToken cancellationToken)
    {
        var clientes = await _clienteService.ListarClientesAsync(cancellationToken);
        return Ok(clientes);
    }

    [HttpGet("documento/{cpfCnpj}")]
    public async Task<ActionResult<ClienteDto>> ObterPorDocumento(string cpfCnpj, CancellationToken cancellationToken)
    {
        var cliente = await _clienteService.ObterClientePorCpfCnpjAsync(cpfCnpj, cancellationToken);
        return Ok(cliente);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClienteDto>> Atualizar(int id, [FromBody] AtualizarClienteDto dto, CancellationToken cancellationToken)
    {
        var cliente = await _clienteService.AtualizarClienteAsync(id, dto, cancellationToken);
        return Ok(cliente);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id, CancellationToken cancellationToken)
    {
        await _clienteService.DeletarClienteAsync(id, cancellationToken);
        return NoContent();
    }
}
