using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fiap.TechChallenge.OficinaMecanica.API.Mappers;
using Fiap.TechChallenge.OficinaMecanica.API.Requests.Clientes;
using Fiap.TechChallenge.OficinaMecanica.API.Responses;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Criar([FromBody] CriarClienteRequest request, CancellationToken cancellationToken)
    {
        var cliente = await _mediator.Send(request.ToCommand(), cancellationToken);
        var response = cliente.ToResponse();
        return CreatedAtAction(nameof(ObterPorDocumento), new { cpfCnpj = response.CpfCnpj }, response);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ClienteResponse>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? nome = null,
        [FromQuery] string? cpfCnpj = null,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

        var clientes = await _mediator.Send(new ListarClientesQuery
        {
            Page = page,
            PageSize = pageSize,
            Nome = nome,
            CpfCnpj = cpfCnpj
        }, cancellationToken);
        return Ok(clientes.ToResponse());
    }

    [HttpGet("documento/{cpfCnpj}")]
    public async Task<ActionResult<ClienteResponse>> ObterPorDocumento(string cpfCnpj, CancellationToken cancellationToken)
    {
        var cliente = await _mediator.Send(cpfCnpj.ToQueryByDocumento(), cancellationToken);
        return Ok(cliente.ToResponse());
    }

    [HttpGet("{cpfCnpj}/veiculos")]
    public async Task<ActionResult<IEnumerable<VeiculoDto>>> ListarVeiculosPorDocumento(string cpfCnpj, CancellationToken cancellationToken)
    {
        var veiculos = await _mediator.Send(new ListarVeiculosPorDocumentoClienteQuery
        {
            CpfCnpj = cpfCnpj
        }, cancellationToken);
        return Ok(veiculos);
    }

    [HttpPost("{cpfCnpj}/veiculos")]
    public async Task<ActionResult<VeiculoDto>> CriarVeiculo(string cpfCnpj, [FromBody] CriarVeiculoParaClienteDto dto, CancellationToken cancellationToken)
    {
        var veiculo = await _mediator.Send(dto.ToCommand(cpfCnpj), cancellationToken);
        return CreatedAtAction("ObterPorPlaca", "Veiculos", new { placa = veiculo.Placa }, veiculo);
    }

    [HttpPut("documento/{cpfCnpj}")]
    public async Task<ActionResult<ClienteResponse>> AtualizarPorDocumento(string cpfCnpj, [FromBody] AtualizarClienteRequest request, CancellationToken cancellationToken)
    {
        var cliente = await _mediator.Send(request.ToCommand(cpfCnpj), cancellationToken);
        return Ok(cliente.ToResponse());
    }

    [HttpDelete("documento/{cpfCnpj}")]
    public async Task<IActionResult> DeletarPorDocumento(string cpfCnpj, CancellationToken cancellationToken)
    {
        await _mediator.Send(cpfCnpj.ToDeleteCommand(), cancellationToken);
        return NoContent();
    }
}
