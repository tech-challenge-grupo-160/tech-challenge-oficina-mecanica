using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Options;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public interface IAuthApplicationService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
}

public class AuthApplicationService : IAuthApplicationService
{
    private const string LoggerName = nameof(AuthApplicationService);
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger _logger;

    public AuthApplicationService(IUsuarioRepository usuarioRepository, IOptions<JwtOptions> jwtOptions, ILoggerFactory loggerFactory)
    {
        _usuarioRepository = usuarioRepository;
        _jwtOptions = jwtOptions.Value;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(LoginAsync), "Consultando usuario para autenticacao");
            var usuario = await _usuarioRepository.ObterPorUsuarioAsync(dto.Usuario, cancellationToken);
            if (usuario == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(LoginAsync), "Usuario nao encontrado para autenticacao");
                throw new UnauthorizedAccessException("Usuário ou senha inválidos.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(LoginAsync), "Validando credenciais do usuario");
            var senhaHash = StringHelper.ToMd5Hash(dto.Senha);
            if (!string.Equals(senhaHash, usuario.SenhaHash, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(LoginAsync), "Credenciais invalidas para o usuario informado");
                throw new UnauthorizedAccessException("Usuário ou senha inválidos.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(LoginAsync), "Gerando token JWT para o usuario autenticado");
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
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Login realizado com sucesso para o usuario {usuario.UsuarioLogin}.");
            return new LoginResponseDto
            {
                Token = tokenString,
                ExpiraEm = expires,
                NomeUsuario = usuario.Nome,
                Role = usuario.Role
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(LoginAsync), ex.Message);
            throw;
        }
    }
}
