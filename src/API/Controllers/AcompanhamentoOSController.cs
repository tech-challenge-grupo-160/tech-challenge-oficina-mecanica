using System.Threading.RateLimiting;
using Fiap.TechChallenge.OficinaMecanica.API.Mappers;
using Fiap.TechChallenge.OficinaMecanica.API.Requests.AcompanhamentoOS;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.AcompanhamentoOS;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fiap.TechChallenge.OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/v1/acompanhamento-os")]
public class AcompanhamentoOSController : ControllerBase
{
    private readonly IMediator _mediator;

    public AcompanhamentoOSController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [EnableRateLimiting("public")]
    [HttpGet("{codigo}")]
    public async Task<ActionResult<AcompanhamentoOrdemDeServicoResponse>> ObterStatus(
        string codigo,
        [FromHeader(Name = "X-Tracking-Token")] string? trackingToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(trackingToken))
        {
            return BadRequest("Header X-Tracking-Token e obrigatorio.");
        }

        var request = new ObterAcompanhamentoOSRequest
        {
            Codigo = codigo,
            Token = trackingToken
        };

        var result = await _mediator.Send(request.ToQuery(), cancellationToken);
        return Ok(result.ToResponse());
    }
}
