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
    private readonly IVeiculoApplicationService _veiculoService;

    public ClientesController(IClienteApplicationService clienteService, IVeiculoApplicationService veiculoService)
    {
        _clienteService = clienteService;
        _veiculoService = veiculoService;
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Criar([FromBody] CriarClienteDto dto, CancellationToken cancellationToken)
    {
        var cliente = await _clienteService.CriarClienteAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObterPorDocumento), new { cpfCnpj = cliente.CpfCnpj }, cliente);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ClienteDto>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? nome = null,
        [FromQuery] string? cpfCnpj = null,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

        var clientes = await _clienteService.ListarClientesAsync(page, pageSize, nome, cpfCnpj, cancellationToken);
        return Ok(clientes);
    }

    [HttpGet("documento/{cpfCnpj}")]
    public async Task<ActionResult<ClienteDto>> ObterPorDocumento(string cpfCnpj, CancellationToken cancellationToken)
    {
        var cliente = await _clienteService.ObterClientePorCpfCnpjAsync(cpfCnpj, cancellationToken);
        return Ok(cliente);
    }

    [HttpGet("{cpfCnpj}/veiculos")]
    public async Task<ActionResult<IEnumerable<VeiculoDto>>> ListarVeiculosPorDocumento(string cpfCnpj, CancellationToken cancellationToken)
    {
        var veiculos = await _veiculoService.ListarVeiculosPorCpfCnpjAsync(cpfCnpj, cancellationToken);
        return Ok(veiculos);
    }

    [HttpPost("{cpfCnpj}/veiculos")]
    public async Task<ActionResult<VeiculoDto>> CriarVeiculo(string cpfCnpj, [FromBody] CriarVeiculoParaClienteDto dto, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoService.CriarVeiculoParaClienteAsync(cpfCnpj, dto, cancellationToken);
        return CreatedAtAction("ObterPorPlaca", "Veiculos", new { placa = veiculo.Placa }, veiculo);
    }

    [HttpPut("documento/{cpfCnpj}")]
    public async Task<ActionResult<ClienteDto>> AtualizarPorDocumento(string cpfCnpj, [FromBody] AtualizarClienteDto dto, CancellationToken cancellationToken)
    {
        var cliente = await _clienteService.AtualizarClientePorCpfCnpjAsync(cpfCnpj, dto, cancellationToken);
        return Ok(cliente);
    }

    [HttpDelete("documento/{cpfCnpj}")]
    public async Task<IActionResult> DeletarPorDocumento(string cpfCnpj, CancellationToken cancellationToken)
    {
        await _clienteService.DeletarClientePorCpfCnpjAsync(cpfCnpj, cancellationToken);
        return NoContent();
    }
}
