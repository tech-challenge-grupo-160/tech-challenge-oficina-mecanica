using FluentAssertions;
using Fiap.TechChallenge.OficinaMecanica.Application;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.Clientes;
using FluentValidation;
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

    [Fact]
    public void AddApplication_DeveRegistrarValidatorsDaApplication()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddApplication();

        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService<IValidator<CriarClienteCommand>>();

        validator.Should().NotBeNull();
    }
}
