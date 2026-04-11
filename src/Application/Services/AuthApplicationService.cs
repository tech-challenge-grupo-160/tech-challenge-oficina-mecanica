using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Options;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface IAuthApplicationService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
}

public class AuthApplicationService : IAuthApplicationService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly JwtOptions _jwtOptions;

    public AuthApplicationService(IUsuarioRepository usuarioRepository, IOptions<JwtOptions> jwtOptions)
    {
        _usuarioRepository = usuarioRepository;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorUsuarioAsync(dto.Usuario, cancellationToken);
        if (usuario == null)
        {
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");
        }

        var senhaHash = StringHelper.ToMd5Hash(dto.Senha);
        if (!string.Equals(senhaHash, usuario.SenhaHash, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");
        }

        var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, usuario.UsuarioLogin),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Role, usuario.Role)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return new LoginResponseDto
        {
            Token = tokenString,
            ExpiraEm = expires,
            NomeUsuario = usuario.Nome,
            Role = usuario.Role
        };
    }
}
