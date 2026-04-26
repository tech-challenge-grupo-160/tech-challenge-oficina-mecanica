using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/v1/acompanhamento-os")]
public class AcompanhamentoOSController : ControllerBase
{
    private readonly IAcompanhamentoOSApplicationService _acompanhamentoService;

    public AcompanhamentoOSController(IAcompanhamentoOSApplicationService acompanhamentoService)
    {
        _acompanhamentoService = acompanhamentoService;
    }

    [AllowAnonymous]
    [HttpGet("{codigo}")]
    public async Task<ActionResult<AcompanhamentoOrdemDeServicoDto>> ObterStatus(
        string codigo,
        [FromHeader(Name = "X-Tracking-Token")] string? trackingToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(trackingToken))
        {
            return BadRequest("Header X-Tracking-Token e obrigatorio.");
        }

        var acompanhamento = await _acompanhamentoService.ObterStatusAsync(codigo, trackingToken, cancellationToken);
        return Ok(acompanhamento);
    }
}
