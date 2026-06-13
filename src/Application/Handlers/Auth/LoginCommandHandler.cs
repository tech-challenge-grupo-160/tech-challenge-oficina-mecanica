using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Auth;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Auth;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Auth;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private const string LoggerName = nameof(LoginCommandHandler);
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ILogger _logger;

    public LoginCommandHandler(
        IUsuarioRepository usuarioRepository,
        ITokenGenerator tokenGenerator,
        ILoggerFactory loggerFactory)
    {
        _usuarioRepository = usuarioRepository;
        _tokenGenerator = tokenGenerator;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<LoginResult> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando usuario para autenticacao");
            var usuario = await _usuarioRepository.ObterPorUsuarioAsync(command.Usuario, cancellationToken);
            if (usuario == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Usuario nao encontrado para autenticacao");
                throw new ServiceUnauthorizedException("Usuario ou senha invalidos.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Validando credenciais do usuario");
            var senhaHash = StringHelper.ToMd5Hash(command.Senha);
            if (!string.Equals(senhaHash, usuario.SenhaHash, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Credenciais invalidas para o usuario informado");
                throw new ServiceUnauthorizedException("Usuario ou senha invalidos.");
            }

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Gerando token para o usuario autenticado");
            var token = _tokenGenerator.Gerar(usuario);

            _logger.LogInformation(LogTemplate.End, LoggerName, $"Login realizado com sucesso para o usuario {usuario.UsuarioLogin}.");
            return new LoginResult
            {
                Token = token.Token,
                ExpiraEm = token.ExpiraEm,
                NomeUsuario = usuario.Nome,
                Role = usuario.Role
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
