using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Validators.Veiculos;
using Fiap.TechChallenge.OficinaMecanica.Application.Validators.OrdensDeServico;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Fiap.TechChallenge.OficinaMecanica.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CriarOrdemDeServicoDto>, CriarOrdemDeServicoDtoValidator>();
        services.AddScoped<IValidator<CancelarOrdemDeServicoDto>, CancelarOrdemDeServicoDtoValidator>();
        services.AddScoped<IValidator<CriarVeiculoDto>, CriarVeiculoDtoValidator>();
        services.AddScoped<IValidator<CriarVeiculoParaClienteDto>, CriarVeiculoParaClienteDtoValidator>();
        services.AddScoped<IValidator<AtualizarVeiculoDto>, AtualizarVeiculoDtoValidator>();

        services.AddScoped<IClienteApplicationService, ClienteApplicationService>();
        services.AddScoped<IVeiculoApplicationService, VeiculoApplicationService>();
        services.AddScoped<IServicoApplicationService, ServicoApplicationService>();
        services.AddScoped<IPecaApplicationService, PecaApplicationService>();
        services.AddScoped<IOrdemDeServicoApplicationService, OrdemDeServicoApplicationService>();
        services.AddScoped<IAuthApplicationService, AuthApplicationService>();
        services.AddScoped<IPedidoCompraApplicationService, PedidoCompraApplicationService>();
        services.AddScoped<IAcompanhamentoOSApplicationService, AcompanhamentoOSApplicationService>();

        return services;
    }
}
