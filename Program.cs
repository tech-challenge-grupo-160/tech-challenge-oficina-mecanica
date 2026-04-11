using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Fiap.TechChallenge.OficinaMecanica.Application.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Options;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Extensions;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.HealthChecks;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
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
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Application Services
builder.Services.AddScoped<IClienteApplicationService, ClienteApplicationService>();
builder.Services.AddScoped<IVeiculoApplicationService, VeiculoApplicationService>();
builder.Services.AddScoped<IServicoApplicationService, ServicoApplicationService>();
builder.Services.AddScoped<IPecaApplicationService, PecaApplicationService>();
builder.Services.AddScoped<IOrdemDeServicoApplicationService, OrdemDeServicoApplicationService>();
builder.Services.AddScoped<IAuthApplicationService, AuthApplicationService>();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health Checks
app.UseHealthChecks();

// Database migrations and seeding with retry
await app.MigrateAndSeedAsync(app.Environment.IsDevelopment());

app.Run();
