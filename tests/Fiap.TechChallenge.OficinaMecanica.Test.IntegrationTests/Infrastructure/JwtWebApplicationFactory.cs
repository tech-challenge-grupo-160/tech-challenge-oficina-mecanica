using Microsoft.Extensions.DependencyInjection;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;

/// <summary>
/// Sobe a API mantendo a autenticacao JWT Bearer real, sem o TestAuthHandler.
/// E o unico ponto do repositorio onde a validacao de token e exercitada de
/// verdade: issuer, audience, assinatura, expiracao e mapeamento de claims.
///
/// Os valores abaixo replicam os defaults da Lambda de autenticacao
/// (<c>JwtOptions.FromEnvironment</c> em tech-challenge-lambda-auth). Se um
/// deles mudar la, os testes desta fixture quebram - que e exatamente o efeito
/// desejado: o contrato entre os dois servicos nao pode mudar em silencio.
/// </summary>
public class JwtWebApplicationFactory : CustomWebApplicationFactory
{
    public const string Issuer = "Fiap.TechChallenge.OficinaMecanica";
    public const string Audience = "Fiap.TechChallenge.OficinaMecanica";

    /// <summary>
    /// Chave exclusiva de teste. A chave real vive no Secrets Manager (issue #46)
    /// e nunca e versionada. O tamanho importa: HMAC-SHA256 exige no minimo 256
    /// bits, e o handler recusa chaves menores.
    /// </summary>
    public const string SecretKey = "chave-de-teste-do-contrato-lambda-api-256bits";

    protected override string JwtIssuer => Issuer;

    protected override string JwtAudience => Audience;

    protected override string JwtSecretKey => SecretKey;

    /// <summary>
    /// Nao faz nada de proposito. O <c>CustomWebApplicationFactory</c> trocaria
    /// a autenticacao pelo TestAuthHandler; aqui a autenticacao real da aplicacao
    /// permanece registrada.
    /// </summary>
    protected override void ConfigureAuthenticationForTests(IServiceCollection services)
    {
    }
}
