using System.Threading.RateLimiting;
using Fiap.TechChallenge.OficinaMecanica.API.Authorization;
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

    [Authorize(Policy = ApiAuthorizationPolicies.Cliente)]
    [EnableRateLimiting("public")]
    [HttpGet("{codigoAcompanhamento}")]
    public async Task<ActionResult<AcompanhamentoOrdemDeServicoResponse>> ObterStatus(
        string codigoAcompanhamento,
        CancellationToken cancellationToken)
    {
        var request = new ObterAcompanhamentoOSRequest
        {
            CodigoAcompanhamento = codigoAcompanhamento
        };

        var result = await _mediator.Send(request.ToQuery(), cancellationToken);
        return Ok(result.ToResponse());
    }
}
