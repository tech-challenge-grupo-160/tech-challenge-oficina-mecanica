using Fiap.TechChallenge.OficinaMecanica.API.Services;
using Fiap.TechChallenge.OficinaMecanica.API.Validators.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Options;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Extensions;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.HealthChecks;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Logging;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsoleFormatter<PlainConsoleFormatter, PlainConsoleFormatterOptions>();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = PlainConsoleFormatter.FormatterName;
});
builder.Logging.Services.Configure<PlainConsoleFormatterOptions>(options =>
{
    options.TimestampFormat = "dd/MM/yyyy HH:mm:ss ";
    options.UseUtcTimestamp = false;
});

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<Fiap.TechChallenge.OficinaMecanica.API.Filters.DomainExceptionFilter>();
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CriarClienteRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API - Sistema de Gestão de Oficina Mecânica",
        Version = "v1",
        Description = "API para gerenciamento de ordens de serviço, clientes, veículos, serviços e peças",
        Contact = new OpenApiContact
        {
            Name = "Oficina Mecânica",
            Email = "contato@oficina.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o Token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
    var secretKey = jwtSection.GetValue<string>("SecretKey") ?? string.Empty;
    if (string.IsNullOrWhiteSpace(secretKey))
    {
        throw new InvalidOperationException("Jwt:SecretKey não configurado.");
    }
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection.GetValue<string>("Issuer"),
        ValidAudience = jwtSection.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUsuarioAutenticadoService, UsuarioAutenticadoService>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

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
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
const string StartupLoggerName = "Startup";

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

var configuredUrls = builder.Configuration["ASPNETCORE_URLS"] ?? string.Empty;
var hasHttpsBinding = configuredUrls.Contains("https://", StringComparison.OrdinalIgnoreCase);

if (hasHttpsBinding)
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health Checks
app.UseHealthChecks();

// Database migrations and seeding with retry
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=postgres;Database=oficina_mecanica;Username=postgres;Password=postgres";
var usingInMemoryDatabase = builder.Environment.IsEnvironment("Testing") ||
    string.Equals(connectionString, "UseInMemory", StringComparison.OrdinalIgnoreCase);

if (!usingInMemoryDatabase)
{
    await app.MigrateAndSeedAsync(app.Environment.IsDevelopment());
}

app.Lifetime.ApplicationStarted.Register(() =>
{
    var serverAddresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
    var addresses = serverAddresses?.Addresses ?? app.Urls;

    foreach (var address in addresses.OrderBy(x => x))
    {
        startupLogger.LogInformation(
            LogTemplate.End,
            StartupLoggerName,
            $"Aplicacao iniciada em: {address}");

        if (app.Environment.IsDevelopment())
        {
            startupLogger.LogInformation(
                LogTemplate.End,
                StartupLoggerName,
                $"Swagger disponivel em: {address.TrimEnd('/')}/swagger/index.html");
        }
    }
});

app.Run();

public partial class Program;
