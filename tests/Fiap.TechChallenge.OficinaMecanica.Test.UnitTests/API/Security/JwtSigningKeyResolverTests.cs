using Fiap.TechChallenge.OficinaMecanica.API.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.API.Security;

/// <summary>
/// Origem da chave de assinatura do JWT na API (issue #46 / F3-12).
///
/// Espelha os testes de <c>JwtOptionsTests</c> na Lambda: os dois validam o
/// mesmo token, entao a precedencia das origens precisa ser identica.
/// </summary>
public sealed class JwtSigningKeyResolverTests
{
    private static IConfiguration Configuracao(params (string chave, string valor)[] valores)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(valores.ToDictionary(v => v.chave, v => (string?)v.valor))
            .Build();
    }

    [Fact]
    public void Resolver_DeveFalharQuandoNenhumaOrigemForConfigurada()
    {
        // Antes da issue #46, o appsettings.json trazia o placeholder
        // "SET_USING_USER_SECRETS_OR_ENV", que passava na checagem de vazio e
        // virava a chave de assinatura sem ninguem perceber.
        var acao = () => JwtSigningKeyResolver.Resolver(Configuracao());

        acao.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:SecretId*")
            .WithMessage("*Jwt:SecretKey*");
    }

    [Fact]
    public void Resolver_DeveLerDoSecretsManagerQuandoSecretIdForInformado()
    {
        var configuracao = Configuracao(("Jwt:SecretId", "tc-grupo160/hom/jwt-signing-key"));
        string? idRecebido = null;

        var chave = JwtSigningKeyResolver.Resolver(configuracao, id =>
        {
            idRecebido = id;
            return "chave-vinda-do-secrets-manager";
        });

        chave.Should().Be("chave-vinda-do-secrets-manager");
        idRecebido.Should().Be("tc-grupo160/hom/jwt-signing-key");
    }

    [Fact]
    public void Resolver_DevePreferirOSecretsManagerQuandoAsDuasOrigensExistirem()
    {
        var configuracao = Configuracao(
            ("Jwt:SecretId", "tc-grupo160/hom/jwt-signing-key"),
            ("Jwt:SecretKey", "chave-local-esquecida"));

        var chave = JwtSigningKeyResolver.Resolver(configuracao, _ => "chave-gerenciada");

        chave.Should().Be("chave-gerenciada");
    }

    [Fact]
    public void Resolver_DeveAceitarChaveLocalQuandoNaoHouverSecretId()
    {
        var configuracao = Configuracao(("Jwt:SecretKey", "chave-de-desenvolvimento-local"));

        var chave = JwtSigningKeyResolver.Resolver(configuracao, _ => "nao-deve-ser-chamado");

        chave.Should().Be("chave-de-desenvolvimento-local");
    }

    [Fact]
    public void Resolver_DevePropagarFalhaDaLeituraDoSegredo()
    {
        var configuracao = Configuracao(("Jwt:SecretId", "segredo-inexistente"));

        var acao = () => JwtSigningKeyResolver.Resolver(
            configuracao,
            id => throw new InvalidOperationException($"Falha ao ler o segredo '{id}'."));

        acao.Should().Throw<InvalidOperationException>().WithMessage("*segredo-inexistente*");
    }
}
