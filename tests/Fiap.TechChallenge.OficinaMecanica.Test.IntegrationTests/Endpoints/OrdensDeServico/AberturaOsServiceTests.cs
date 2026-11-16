using System.Net;
using System.Net.Http.Json;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Endpoints.OrdensDeServico;

public class AberturaOsServiceTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AberturaOsServiceTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        factory.ResetDatabase();
    }

    [Fact]
    public async Task Abrir_DevePreservarSnapshotsDosItensSemCatalogoLocal()
    {
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/ordens-servico", CriarPayload(quantidade: 101));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var ordem = await response.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        ordem.Should().NotBeNull();
        ordem!.Servicos.Should().ContainSingle(x =>
            x.ServicoId == CustomWebApplicationFactory.ServicoExistenteId &&
            x.Preco == 150m &&
            x.TempoEstimado == 30);
        ordem.Pecas.Should().ContainSingle(x =>
            x.PecaId == CustomWebApplicationFactory.PecaExistenteId &&
            x.Quantidade == 101 &&
            x.Preco == 45m);
        ordem.ValorTotal.Should().Be(4695m);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
            var servico = await context.OrdemDeServicoServicos
                .SingleAsync(x => x.OrdemDeServicoId == ordem.Id);
            servico.ServicoId.Should().Be(CustomWebApplicationFactory.ServicoExistenteId);
            servico.Preco.Should().Be(150m);
            servico.TempoEstimado.Should().Be(30);

            var peca = await context.OrdemDeServicoPecas
                .SingleAsync(x => x.OrdemDeServicoId == ordem.Id);
            peca.PecaId.Should().Be(CustomWebApplicationFactory.PecaExistenteId);
            peca.Quantidade.Should().Be(101);
            peca.Preco.Should().Be(45m);
        }

        var consulta = await client.GetFromJsonAsync<OrdemDeServicoResponse>($"/api/v1/ordens-servico/{ordem.Id}");
        consulta!.ValorTotal.Should().Be(4695m);
        consulta.Servicos.Should().ContainSingle(x => x.Preco == 150m && x.TempoEstimado == 30);
    }

    [Fact]
    public async Task Abrir_DeveReverterTodaEscritaQuandoNotificacaoFalhar()
    {
        using var factoryComFalha = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<INotificacaoClienteRepository>();
                services.AddScoped<INotificacaoClienteRepository>(provider =>
                    new RepositorioNotificacaoQueFalha(
                        new NotificacaoClienteRepository(provider.GetRequiredService<OficinaDbContext>())));
            }));
        using var client = factoryComFalha.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/ordens-servico", CriarPayload(quantidade: 2));

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        using var scope = factoryComFalha.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
        (await context.OrdensDeServico.CountAsync()).Should().Be(0);
        (await context.OrdemDeServicoServicos.CountAsync()).Should().Be(0);
        (await context.OrdemDeServicoPecas.CountAsync()).Should().Be(0);
        (await context.OrdemServicoHistoricos.CountAsync()).Should().Be(0);
        (await context.NotificacoesCliente.CountAsync()).Should().Be(0);
    }

    private static object CriarPayload(int quantidade) => new
    {
        clienteId = CustomWebApplicationFactory.PessoaFisicaClienteId,
        veiculoId = CustomWebApplicationFactory.VeiculoExistenteId,
        descricaoSolicitacao = "Ruido na suspensao dianteira.",
        observacoesRecepcao = "Validar alinhamento e pastilhas.",
        servicos = new[] { new { servicoId = CustomWebApplicationFactory.ServicoExistenteId } },
        pecas = new[] { new { pecaId = CustomWebApplicationFactory.PecaExistenteId, quantidade } }
    };

    private sealed class RepositorioNotificacaoQueFalha(INotificacaoClienteRepository inner)
        : INotificacaoClienteRepository
    {
        public async Task<NotificacaoCliente> CriarAsync(
            NotificacaoCliente notificacao, CancellationToken cancellationToken)
        {
            await inner.CriarAsync(notificacao, cancellationToken);
            throw new IOException("Falha simulada depois da gravacao da notificacao.");
        }

        public Task<IEnumerable<NotificacaoCliente>> ObterPorOrdemDeServicoAsync(
            int ordemDeServicoId, CancellationToken cancellationToken) =>
            inner.ObterPorOrdemDeServicoAsync(ordemDeServicoId, cancellationToken);
    }
}
