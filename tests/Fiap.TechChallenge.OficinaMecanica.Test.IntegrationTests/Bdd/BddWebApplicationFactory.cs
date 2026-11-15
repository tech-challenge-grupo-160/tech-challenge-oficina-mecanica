using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Bdd;

/// <summary>
/// Mesma API dos testes de integracao (Postgres no Testcontainers, catalogo da
/// Execucao local, autenticacao de teste), mas com o agendador real do inicio
/// do diagnostico: e ele que cria a saga e grava o primeiro comando no outbox.
/// O SQS fica desligado; o BDD entrega as mensagens de Billing e Execucao
/// direto aos consumidores do OS Service.
/// </summary>
public sealed class BddWebApplicationFactory : CustomWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IInicioDiagnosticoScheduler>();
            services.AddScoped<IInicioDiagnosticoScheduler, InicioDiagnosticoScheduler>();
        });
    }
}
