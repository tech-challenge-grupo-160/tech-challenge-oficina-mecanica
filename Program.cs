using Microsoft.EntityFrameworkCore;
using oficina_mecanica.Application.Services;
using oficina_mecanica.Domain.Repositories;
using oficina_mecanica.Infrastructure.Data;
using oficina_mecanica.Infrastructure.Extensions;
using oficina_mecanica.Infrastructure.HealthChecks;
using oficina_mecanica.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API - Sistema de Gestão de Oficina Mecânica",
        Version = "v1",
        Description = "API para gerenciamento de ordens de serviço, clientes, veículos, serviços e peças",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Oficina Mecânica",
            Email = "contato@oficina.com"
        }
    });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=postgres;Database=oficina_mecanica;Username=postgres;Password=postgres";
builder.Services.AddDbContext<OficinaDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// Health Checks
builder.AddHealthChecks();

// Repositories
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IPecaRepository, PecaRepository>();
builder.Services.AddScoped<IOrdemDeServicoRepository, OrdemDeServicoRepository>();

// Application Services
builder.Services.AddScoped<IClienteApplicationService, ClienteApplicationService>();
builder.Services.AddScoped<IVeiculoApplicationService, VeiculoApplicationService>();
builder.Services.AddScoped<IServicoApplicationService, ServicoApplicationService>();
builder.Services.AddScoped<IPecaApplicationService, PecaApplicationService>();
builder.Services.AddScoped<IOrdemDeServicoApplicationService, OrdemDeServicoApplicationService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Health Checks
app.UseHealthChecks();

// Database migrations and seeding with retry
await app.MigrateAndSeedAsync(app.Environment.IsDevelopment());

app.Run();
