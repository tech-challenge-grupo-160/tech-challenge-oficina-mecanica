using Fiap.TechChallenge.OficinaMecanica.API.Bootstrap;
using Fiap.TechChallenge.OficinaMecanica.Application;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureApiLogging();

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseApiPipeline(builder.Configuration);
await app.InitializeDatabaseAsync();
app.LogStartupUrls();

app.Run();

public partial class Program;
