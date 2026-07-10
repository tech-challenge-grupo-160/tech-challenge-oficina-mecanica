using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Auth;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Auth;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Application.Validators.Auth;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Handlers.Auth;

public class AuthHandlersTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;

    public AuthHandlersTests()
    {
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        _tokenGeneratorMock = new Mock<ITokenGenerator>(MockBehavior.Strict);
        _passwordHasherMock = new Mock<IPasswordHasher>(MockBehavior.Strict);
    }

    [Fact]
    public async Task Login_DeveRetornarTokenQuandoCredenciaisForemValidas()
    {
        var usuario = Usuario.Criar("Administrador", "admin", "hash-senha", "Admin");
        var expiraEm = new DateTime(2026, 6, 4, 12, 0, 0);

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorUsuarioAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _passwordHasherMock
            .Setup(x => x.Verify("senha-correta", usuario.SenhaHash))
            .Returns(true);
        _tokenGeneratorMock
            .Setup(x => x.Gerar(usuario))
            .Returns(new TokenResult
            {
                Token = "jwt-token",
                ExpiraEm = expiraEm
            });

        var handler = CriarHandler();

        var resultado = await handler.Handle(new LoginCommand
        {
            Usuario = "admin",
            Senha = "senha-correta"
        }, CancellationToken.None);

        resultado.Token.Should().Be("jwt-token");
        resultado.ExpiraEm.Should().Be(expiraEm);
        resultado.NomeUsuario.Should().Be("Administrador");
        resultado.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_DeveLancarUnauthorizedQuandoUsuarioNaoExistir()
    {
        _usuarioRepositoryMock
            .Setup(x => x.ObterPorUsuarioAsync("inexistente", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var handler = CriarHandler();

        var acao = () => handler.Handle(new LoginCommand
        {
            Usuario = "inexistente",
            Senha = "qualquer"
        }, CancellationToken.None);

        await acao.Should().ThrowAsync<ServiceUnauthorizedException>()
            .WithMessage("Usuario ou senha invalidos.");
        _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.Gerar(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task Login_DeveLancarUnauthorizedQuandoSenhaForInvalida()
    {
        var usuario = Usuario.Criar("Administrador", "admin", "hash-senha", "Admin");

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorUsuarioAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _passwordHasherMock
            .Setup(x => x.Verify("senha-incorreta", usuario.SenhaHash))
            .Returns(false);

        var handler = CriarHandler();

        var acao = () => handler.Handle(new LoginCommand
        {
            Usuario = "admin",
            Senha = "senha-incorreta"
        }, CancellationToken.None);

        await acao.Should().ThrowAsync<ServiceUnauthorizedException>()
            .WithMessage("Usuario ou senha invalidos.");
        _tokenGeneratorMock.Verify(x => x.Gerar(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public void LoginValidator_DeveRejeitarUsuarioESenhaVazios()
    {
        var validator = new LoginCommandValidator();

        var resultado = validator.TestValidate(new LoginCommand
        {
            Usuario = "",
            Senha = ""
        });

        resultado.ShouldHaveValidationErrorFor(x => x.Usuario)
            .WithErrorMessage("Usuario e obrigatorio.");
        resultado.ShouldHaveValidationErrorFor(x => x.Senha)
            .WithErrorMessage("Senha e obrigatoria.");
    }

    private LoginCommandHandler CriarHandler()
    {
        return new LoginCommandHandler(
            _usuarioRepositoryMock.Object,
            _tokenGeneratorMock.Object,
            _passwordHasherMock.Object,
            NullLoggerFactory.Instance);
    }
}
