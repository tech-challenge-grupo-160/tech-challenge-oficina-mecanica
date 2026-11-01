using System.Net;
using System.Text;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Catalogo;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Infrastructure;

public sealed class CatalogoExecucaoHttpClientTests
{
    [Fact]
    public async Task ObterItensAsync_DeveMapearContratoDaExecucao()
    {
        string? requestBody = null;
        var handler = new StubHttpMessageHandler(async request =>
        {
            requestBody = await request.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                      "servicos": [{"id":1000,"nome":"Alinhamento","preco":150,"tempoEstimado":30}],
                      "pecas": [{"id":1000,"nome":"Pastilha","preco":45}],
                      "servicoIdsNaoEncontrados": [],
                      "pecaIdsNaoEncontrados": []
                    }
                    """, Encoding.UTF8, "application/json")
            };
        });
        var client = new CatalogoExecucaoHttpClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://execucao/")
        });

        var result = await client.ObterItensAsync([1000, 1000], [1000], CancellationToken.None);

        result.Servicos.Should().ContainSingle(item =>
            item.Id == 1000 && item.Preco == 150m && item.TempoEstimado == 30);
        result.Pecas.Should().ContainSingle(item => item.Id == 1000 && item.Preco == 45m);
        requestBody.Should().Contain("\"servicoIds\":[1000]");
        requestBody.Should().Contain("\"pecaIds\":[1000]");
    }

    [Fact]
    public async Task ObterItensAsync_DeveConverterFalhaHttpEmIndisponibilidade()
    {
        var handler = new StubHttpMessageHandler(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.InternalServerError)));
        var client = new CatalogoExecucaoHttpClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://execucao/")
        });

        var action = () => client.ObterItensAsync([1000], [], CancellationToken.None);

        var exception = await action.Should().ThrowAsync<ServiceUnavailableException>();
        exception.Which.StatusCode.Should().Be(503);
    }

    [Fact]
    public async Task ObterItensAsync_DeveRejeitarContratoInvalido()
    {
        var handler = new StubHttpMessageHandler(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            }));
        var client = new CatalogoExecucaoHttpClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://execucao/")
        });

        var action = () => client.ObterItensAsync([1000], [], CancellationToken.None);

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
