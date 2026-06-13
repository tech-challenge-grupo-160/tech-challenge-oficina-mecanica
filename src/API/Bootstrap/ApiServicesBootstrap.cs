using Fiap.TechChallenge.OficinaMecanica.API.Filters;
using Fiap.TechChallenge.OficinaMecanica.API.Services;
using Fiap.TechChallenge.OficinaMecanica.API.Validators.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.Options;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace Fiap.TechChallenge.OficinaMecanica.API.Bootstrap;

public static class ApiServicesBootstrap
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<DomainExceptionFilter>();
        });
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<CriarClienteRequestValidator>();
        services.AddEndpointsApiExplorer();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddApiAuthentication(configuration);
        services.AddApiSwagger();

        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped<IUsuarioAutenticadoService, UsuarioAutenticadoService>();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}
