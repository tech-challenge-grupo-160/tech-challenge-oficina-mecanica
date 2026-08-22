using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;

namespace Fiap.TechChallenge.OficinaMecanica.API.Services;

public class UsuarioAutenticadoService : IUsuarioAutenticadoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioAutenticadoService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UsuarioAutenticadoInfo ObterUsuarioAtual()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return new UsuarioAutenticadoInfo();
        }

        var usuarioId =
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            user.FindFirstValue("sub");

        var usuarioNome =
            user.FindFirstValue(ClaimTypes.Name) ??
            user.FindFirstValue(JwtRegisteredClaimNames.UniqueName) ??
            user.FindFirstValue("unique_name");

        var clienteDocumento = user.FindFirstValue("documento");

        return new UsuarioAutenticadoInfo
        {
            UsuarioId = usuarioId,
            UsuarioNome = usuarioNome,
            ClienteDocumento = clienteDocumento
        };
    }
}
