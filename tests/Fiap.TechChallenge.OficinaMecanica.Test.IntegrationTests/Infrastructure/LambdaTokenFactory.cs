using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;

/// <summary>
/// Reproduz fielmente o token emitido pela Lambda de autenticacao.
///
/// Espelha <c>JwtTokenGenerator.Gerar</c> de tech-challenge-lambda-auth: mesmo
/// conjunto de claims, mesma ordem, mesmo algoritmo. Os dois servicos vivem em
/// repositorios separados e nao compartilham codigo, entao este arquivo e a
/// copia do contrato do lado da API.
///
/// Ao mudar as claims da Lambda, mude aqui tambem - e o teste que falhar avisa
/// que a mudanca quebra a API.
/// </summary>
public static class LambdaTokenFactory
{
    public const int ClienteId = 42;
    public const string ClienteDocumento = "47654866801";
    public const string ClienteNome = "Vanessa Luna Duarte";
    public const string RoleCliente = "Cliente";

    /// <summary>
    /// Token valido, equivalente ao que a Lambda devolve para um cliente ativo.
    /// </summary>
    public static string Gerar(
        string? issuer = null,
        string? audience = null,
        string? secretKey = null,
        string role = RoleCliente,
        string documento = ClienteDocumento,
        TimeSpan? validoPor = null)
    {
        var expires = DateTime.UtcNow.Add(validoPor ?? TimeSpan.FromMinutes(60));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, ClienteId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, documento),
            new(ClaimTypes.Name, ClienteNome),
            new(ClaimTypes.Role, role),
            new("documento", documento),
            new("tipo_documento", documento.Length > 11 ? "CNPJ" : "CPF"),
            new("status", "Ativo")
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey ?? JwtWebApplicationFactory.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer ?? JwtWebApplicationFactory.Issuer,
            audience: audience ?? JwtWebApplicationFactory.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Token ja expirado. O <c>expires</c> no passado sozinho nao basta: sem o
    /// <c>notBefore</c> tambem no passado o handler recusa por ordem invalida de
    /// datas, e o teste passaria pelo motivo errado.
    /// </summary>
    public static string GerarExpirado()
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, ClienteId.ToString()),
            new(ClaimTypes.Role, RoleCliente),
            new("documento", ClienteDocumento)
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(JwtWebApplicationFactory.SecretKey));

        var token = new JwtSecurityToken(
            issuer: JwtWebApplicationFactory.Issuer,
            audience: JwtWebApplicationFactory.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddHours(-2),
            expires: DateTime.UtcNow.AddHours(-1),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
