using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Endpoints.Auth;

/// <summary>
/// Contrato entre a Lambda de autenticacao e a API .NET (issue #45 / F3-11).
///
/// Diferente dos demais testes de integracao, esta classe usa a
/// <see cref="JwtWebApplicationFactory"/>, que mantem a autenticacao JWT Bearer
/// real. O objetivo nao e testar o endpoint, e sim provar que o token emitido
/// pela Lambda atravessa a validacao da API - e que um token adulterado nao.
/// </summary>
public class LambdaJwtContractTests : IClassFixture<JwtWebApplicationFactory>
{
    private const string RotaAutenticada = "/api/v1/pecas";
    private const string RotaDeCliente = "/api/v1/acompanhamento-os/OS-INEXISTENTE";

    private readonly JwtWebApplicationFactory _factory;

    public LambdaJwtContractTests(JwtWebApplicationFactory factory)
    {
        factory.ResetDatabase();
        _factory = factory;
    }

    private HttpClient ClientComToken(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // ------------------------------------------------------------- token valido

    [Fact]
    public async Task TokenDaLambda_DeveSerAceitoEmRotaAutenticada()
    {
        var response = await ClientComToken(LambdaTokenFactory.Gerar()).GetAsync(RotaAutenticada);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TokenDaLambda_DeveSatisfazerAPolicyDeCliente()
    {
        // A rota exige role Cliente. Um codigo de acompanhamento inexistente e
        // suficiente: o que importa e nao ser barrado antes de chegar ao handler.
        var response = await ClientComToken(LambdaTokenFactory.Gerar()).GetAsync(RotaDeCliente);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TokenSemRoleDeCliente_DeveReceberForbiddenNaRotaDeCliente()
    {
        // Autenticado, porem sem a claim de papel exigida: 403, nao 401.
        // A distincao importa - 401 significaria que a autenticacao falhou.
        var token = LambdaTokenFactory.Gerar(role: "Administrador");

        var response = await ClientComToken(token).GetAsync(RotaDeCliente);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ------------------------------------------------------------ token invalido

    [Fact]
    public async Task TokenAssinadoComOutraChave_DeveSerRejeitado()
    {
        var token = LambdaTokenFactory.Gerar(
            secretKey: "outra-chave-completamente-diferente-256bits");

        var response = await ClientComToken(token).GetAsync(RotaAutenticada);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenComIssuerDivergente_DeveSerRejeitado()
    {
        var token = LambdaTokenFactory.Gerar(issuer: "emissor-desconhecido");

        var response = await ClientComToken(token).GetAsync(RotaAutenticada);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenComAudienceDivergente_DeveSerRejeitado()
    {
        var token = LambdaTokenFactory.Gerar(audience: "outra-audiencia");

        var response = await ClientComToken(token).GetAsync(RotaAutenticada);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenExpirado_DeveSerRejeitado()
    {
        var response = await ClientComToken(LambdaTokenFactory.GerarExpirado())
            .GetAsync(RotaAutenticada);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequisicaoSemToken_DeveSerRejeitada()
    {
        var response = await _factory.CreateClient().GetAsync(RotaAutenticada);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenMalformado_DeveSerRejeitado()
    {
        var response = await ClientComToken("nao.e.um.jwt").GetAsync(RotaAutenticada);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ------------------------------------------------- claims no contexto

    [Fact]
    public async Task ClaimDocumento_DeveChegarAoHandlerPeloTokenDaLambda()
    {
        // Prova que a claim "documento" sobrevive a validacao do JWT e chega ao
        // UsuarioAutenticadoService: o handler compara o documento do token com
        // o cliente dono da ordem antes de liberar o acompanhamento.
        var client = ClientComToken(LambdaTokenFactory.Gerar());
        var codigo = await CriarOrdemEObterCodigoAsync(client);

        var response = await client.GetAsync($"/api/v1/acompanhamento-os/{codigo}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ClaimDocumentoDeOutroCliente_NaoDeveAcessarAOrdem()
    {
        // Mesma rota, mesmo token valido, documento de outro cliente: a ordem
        // existe, mas o handler recusa. Confirma que a comparacao usa o valor
        // que veio do token, e nao um default.
        var clienteDono = ClientComToken(LambdaTokenFactory.Gerar());
        var codigo = await CriarOrdemEObterCodigoAsync(clienteDono);

        var outroCliente = ClientComToken(LambdaTokenFactory.Gerar(documento: "60617051000199"));
        var response = await outroCliente.GetAsync($"/api/v1/acompanhamento-os/{codigo}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<string> CriarOrdemEObterCodigoAsync(HttpClient client)
    {
        var payload = new
        {
            clienteId = CustomWebApplicationFactory.PessoaFisicaClienteId,
            veiculoId = CustomWebApplicationFactory.VeiculoExistenteId,
            descricaoSolicitacao = "Cliente relatou barulho na suspensao.",
            observacoesRecepcao = "Validar alinhamento e folgas.",
            servicos = new[]
            {
                new { servicoId = CustomWebApplicationFactory.ServicoExistenteId }
            },
            pecas = Array.Empty<object>()
        };

        var response = await client.PostAsJsonAsync("/api/v1/ordens-servico", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var ordem = await response.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        ordem.Should().NotBeNull();
        ordem!.CodigoAcompanhamento.Should().NotBeNullOrWhiteSpace();
        return ordem.CodigoAcompanhamento!;
    }
}
