using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace Fiap.TechChallenge.OficinaMecanica.API.Security;

/// <summary>
/// Resolve a chave de assinatura do JWT (issue #46 / F3-12).
///
/// Espelha deliberadamente a logica de <c>JwtOptions.FromEnvironment</c> da
/// Lambda de autenticacao: os dois validam o mesmo token e precisam da mesma
/// chave, entao a ordem de precedencia tambem precisa ser a mesma.
///
/// 1. <c>Jwt:SecretId</c> - nome do segredo no Secrets Manager. Caminho usado
///    no cluster: a configuracao carrega o identificador, nunca o valor.
/// 2. <c>Jwt:SecretKey</c> - a chave em claro. Execucao local e testes.
///
/// Sem nenhuma das duas o startup falha. Nao existe valor padrao.
/// </summary>
public static class JwtSigningKeyResolver
{
    public static string Resolver(IConfiguration configuration, Func<string, string>? lerSegredo = null)
    {
        var jwtSection = configuration.GetSection("Jwt");

        var secretId = jwtSection.GetValue<string>("SecretId");
        if (!string.IsNullOrWhiteSpace(secretId))
        {
            return (lerSegredo ?? LerDoSecretsManager)(secretId);
        }

        var secretKey = jwtSection.GetValue<string>("SecretKey");
        if (!string.IsNullOrWhiteSpace(secretKey))
        {
            return secretKey;
        }

        throw new InvalidOperationException(
            "Chave de assinatura do JWT nao configurada. Defina Jwt:SecretId com o nome " +
            "do segredo no Secrets Manager, ou Jwt:SecretKey para execucao local.");
    }

    /// <summary>
    /// Leitura sincrona porque acontece no startup, antes de qualquer
    /// requisicao. Uma rotacao do segredo exige reiniciar os pods - limitacao
    /// registrada na RFC-0002.
    /// </summary>
    private static string LerDoSecretsManager(string secretId)
    {
        GetSecretValueResponse response;
        try
        {
            using var client = new AmazonSecretsManagerClient();
            response = client
                .GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretId })
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            // A mensagem nomeia o segredo, nunca o valor.
            throw new InvalidOperationException(
                $"Falha ao ler o segredo '{secretId}' no Secrets Manager.", ex);
        }

        if (string.IsNullOrWhiteSpace(response.SecretString))
        {
            throw new InvalidOperationException($"O segredo '{secretId}' existe mas esta vazio.");
        }

        return response.SecretString;
    }
}
