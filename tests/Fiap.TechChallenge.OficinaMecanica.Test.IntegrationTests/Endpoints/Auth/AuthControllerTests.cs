using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.Auth;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Endpoints.Auth;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_DeveRetornarTokenQuandoCredenciaisForemValidas()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new
            {
                usuario = CustomWebApplicationFactory.UsuarioLogin,
                senha = CustomWebApplicationFactory.UsuarioSenha
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        login.Should().NotBeNull();
        login!.Token.Should().NotBeNullOrWhiteSpace();
        login.NomeUsuario.Should().Be("Administrador Integracao");
        login.Role.Should().Be("Administrador");
        login.ExpiraEm.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_DeveRetornarUnauthorizedProblemDetailsQuandoCredenciaisForemInvalidas()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new
            {
                usuario = CustomWebApplicationFactory.UsuarioLogin,
                senha = "senha-incorreta"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        await using var body = await response.Content.ReadAsStreamAsync();
        using var problemDetails = await JsonDocument.ParseAsync(body);
        var root = problemDetails.RootElement;

        root.GetProperty("status").GetInt32().Should().Be((int)HttpStatusCode.Unauthorized);
        root.GetProperty("detail").GetString().Should().Contain("Usuario ou senha invalidos");
        root.GetProperty("traceId").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_DeveRetornarBadRequestProblemDetailsQuandoPayloadForInvalido()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new
            {
                usuario = "",
                senha = ""
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadAsStringAsync();
        problem.Should().Contain("Usuario e obrigatorio");
        problem.Should().Contain("Senha e obrigatoria");
    }
}
