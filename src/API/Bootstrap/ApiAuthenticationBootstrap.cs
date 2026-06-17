using Fiap.TechChallenge.OficinaMecanica.Application.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Fiap.TechChallenge.OficinaMecanica.API.Bootstrap;

public static class ApiAuthenticationBootstrap
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var jwtSection = configuration.GetSection(JwtOptions.SectionName);
            var secretKey = jwtSection.GetValue<string>("SecretKey") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("Jwt:SecretKey nao configurado.");
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

        return services;
    }
}
