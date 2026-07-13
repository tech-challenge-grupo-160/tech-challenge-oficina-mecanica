using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Fiap.TechChallenge.OficinaMecanica.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        RegisterValidatorsFromAssembly(services, typeof(DependencyInjection).Assembly);

        services.AddScoped<OrdemDeServicoAcompanhamentoService>();
        services.AddScoped<OrdemDeServicoHistoricoService>();
        services.AddScoped<OrdemDeServicoNotificacaoService>();
        services.AddScoped<OrdemDeServicoEstoqueService>();

        return services;
    }

    private static void RegisterValidatorsFromAssembly(IServiceCollection services, Assembly assembly)
    {
        var validatorTypes = assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Select(type => new
            {
                ImplementationType = type,
                ServiceTypes = type.GetInterfaces()
                    .Where(@interface => @interface.IsGenericType &&
                                         @interface.GetGenericTypeDefinition() == typeof(IValidator<>))
                    .ToArray()
            })
            .Where(x => x.ServiceTypes.Length > 0);

        foreach (var validator in validatorTypes)
        {
            foreach (var serviceType in validator.ServiceTypes)
            {
                services.AddScoped(serviceType, validator.ImplementationType);
            }
        }
    }
}
