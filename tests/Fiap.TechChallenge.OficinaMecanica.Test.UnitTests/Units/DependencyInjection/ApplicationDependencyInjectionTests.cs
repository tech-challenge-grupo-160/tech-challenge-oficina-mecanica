using FluentAssertions;
using Fiap.TechChallenge.OficinaMecanica.Application;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.DependencyInjection;

public class ApplicationDependencyInjectionTests
{
    [Fact]
    public void AddApplication_DeveRegistrar_IMediator()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddApplication();

        using var provider = services.BuildServiceProvider();

        var mediator = provider.GetService<IMediator>();

        mediator.Should().NotBeNull();
    }
}
