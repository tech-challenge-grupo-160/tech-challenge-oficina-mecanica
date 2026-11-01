using System.Net;
using System.Text;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Execucao;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Infrastructure;

public sealed class DiagnosticoExecucaoHttpClientTests
{
    [Fact]
    public async Task IniciarAsync_DeveEnviarChaveIdempotenteEMapearResposta()
    {
        string? chave = null;
        string? corpo = null;
        var handler = new StubHttpMessageHandler(async request =>
        {
            chave = request.Headers.GetValues("Idempotency-Key").Single();
            corpo = await request.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""
                    {
                      "id":42,
                      "ordemServicoId":3001,
                      "veiculoId":1001,
                      "descricaoSolicitacao":"Ruido",
                      "servicoIdsSolicitados":[1000],
                      "status":"EmDiagnostico",
                      "iniciadoEm":"2026-11-01T18:00:00Z"
                    }
                    """, Encoding.UTF8, "application/json")
            };
        });
        var client = new DiagnosticoExecucaoHttpClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://execucao/")
        });

        var result = await client.IniciarAsync(3001, 1001, "Ruido", [1000, 1000], [2000], CancellationToken.None);

        result.Id.Should().Be(42);
        result.OrdemServicoId.Should().Be(3001);
        chave.Should().Be("iniciar-diagnostico:3001");
        corpo.Should().Contain("\"servicoIdsSolicitados\":[1000]");
        corpo.Should().Contain("\"pecaIdsSolicitadas\":[2000]");
    }

    [Fact]
    public async Task IniciarAsync_DeveConverterFalhaRemotaEmIndisponibilidade()
    {
        var handler = new StubHttpMessageHandler(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.InternalServerError)));
        var client = new DiagnosticoExecucaoHttpClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://execucao/")
        });

        var action = () => client.IniciarAsync(3001, 1001, "Ruido", [1000], [], CancellationToken.None);

        await action.Should().ThrowAsync<ServiceUnavailableException>();
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => responseFactory(request);
    }
}
