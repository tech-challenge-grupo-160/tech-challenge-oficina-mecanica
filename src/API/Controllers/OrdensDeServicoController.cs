using Microsoft.AspNetCore.Mvc;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/v1/ordens-servico")]
public class OrdensDeServicoController : ControllerBase
{
    private readonly IOrdemDeServicoApplicationService _ordemService;

    public OrdensDeServicoController(IOrdemDeServicoApplicationService ordemService)
    {
        _ordemService = ordemService;
    }

    [HttpPost]
    public async Task<ActionResult<OrdemDeServicoDto>> Criar([FromBody] CriarOrdemDeServicoDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _ordemService.CriarOrdemDeServicoAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = ordem.Id }, ordem);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrdemDeServicoDto>> Obter(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemService.ObterOrdemDeServicoAsync(id, cancellationToken);
        return Ok(ordem);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<OrdemDeServicoDto>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? clienteId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? numero = null,
        [FromQuery] DateTime? dataAberturaInicio = null,
        [FromQuery] DateTime? dataAberturaFim = null,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0)
        {
            return BadRequest("page deve ser maior que zero.");
        }

        if (pageSize <= 0)
        {
            return BadRequest("pageSize deve ser maior que zero.");
        }

        var ordens = await _ordemService.ListarOrdensDeServicoAsync(
            page,
            pageSize,
            clienteId,
            status,
            numero,
            dataAberturaInicio,
            dataAberturaFim,
            cancellationToken);
        return Ok(ordens);
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<IEnumerable<OrdemDeServicoDto>>> ListarPorCliente(int clienteId, CancellationToken cancellationToken)
    {
        var ordens = await _ordemService.ListarOrdensDeServicoPorClienteAsync(clienteId, cancellationToken);
        return Ok(ordens);
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<OrdemDeServicoDto>>> ListarPorStatus(string status, CancellationToken cancellationToken)
    {
        var ordens = await _ordemService.ListarOrdensDeServicoPorStatusAsync(status, cancellationToken);
        return Ok(ordens);
    }

    [HttpPatch("{id}/iniciar-diagnostico")]
    public async Task<ActionResult<OrdemDeServicoDto>> IniciarDiagnostico(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemService.IniciarDiagnosticoAsync(id, cancellationToken);
        return Ok(ordem);
    }

    [HttpPatch("{id}/finalizar-diagnostico")]
    public async Task<ActionResult<OrdemDeServicoDto>> FinalizarDiagnostico(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemService.FinalizarDiagnosticoAsync(id, cancellationToken);
        return Ok(ordem);
    }

    [HttpPatch("{id}/cancelar")]
    public async Task<ActionResult<OrdemDeServicoDto>> Cancelar(int id, [FromBody] CancelarOrdemDeServicoDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _ordemService.CancelarAsync(id, dto, cancellationToken);
        return Ok(ordem);
    }

    [HttpPost("{id}/servicos")]
    public async Task<ActionResult<OrdemDeServicoDto>> AdicionarServico(int id, [FromBody] AdicionarServicoAOrdemDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _ordemService.AdicionarServicoAsync(id, dto, cancellationToken);
        return Ok(ordem);
    }

    [HttpPost("{id}/pecas")]
    public async Task<ActionResult<OrdemDeServicoDto>> AdicionarPeca(int id, [FromBody] AdicionarPecaAOrdemDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _ordemService.AdicionarPecaAsync(id, dto, cancellationToken);
        return Ok(ordem);
    }

}
