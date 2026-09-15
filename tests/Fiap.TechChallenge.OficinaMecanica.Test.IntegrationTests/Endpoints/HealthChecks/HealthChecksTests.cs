using System.Net;
using System.Text.Json;
using Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Endpoints.HealthChecks;

public class HealthChecksTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthChecksTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Liveness_DeveRetornarOkSemConsultarDependencias()
    {
        var response = await _client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Readiness_DeveRetornarOkEIncluirBanco()
    {
        var response = await _client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);

        document.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        document.RootElement.GetProperty("checks")
            .EnumerateArray()
            .Should()
            .Contain(check => check.GetProperty("name").GetString() == "Database");
    }
}
