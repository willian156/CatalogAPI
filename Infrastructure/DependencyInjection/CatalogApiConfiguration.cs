using System.Text;
using CatalogAPI.Application.Catalog;
using CatalogAPI.Domain.Catalog;
using CatalogAPI.Infrastructure.Messaging;
using CatalogAPI.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CatalogAPI.Infrastructure.DependencyInjection;

public static class CatalogApiConfiguration
{
    public static IServiceCollection AddCatalogApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCatalogApplication();
        services.AddCatalogPersistence(configuration);
        services.AddCatalogMessaging(configuration);
        services.AddCatalogAuthentication(configuration);
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerWithBearerAuthentication();
        return services;
    }

    public static WebApplication UseCatalogApi(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }

    public static async Task InitializeCatalogDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.EnsureCreatedAsync();
    }

    private static void AddCatalogApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssemblyContaining<CreateGameCommand>()
        );
    }

    private static void AddCatalogPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database"))
        );
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
    }

    private static void AddCatalogMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<PaymentProcessedConsumer>();
            configurator.UsingRabbitMq((context, rabbit) =>
            {
                ConfigureRabbitHost(rabbit, configuration);
                rabbit.ReceiveEndpoint(
                    configuration["RabbitMq:PaymentQueue"] ?? "catalog-payment-results",
                    endpoint => endpoint.ConfigureConsumer<PaymentProcessedConsumer>(context)
                );
            });
        });
    }

    private static void AddCatalogAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var key = configuration["Jwt:Key"] ?? "change-this-development-key-with-32-chars";
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "FCG",
                    ValidAudience = configuration["Jwt:Audience"] ?? "FCG",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                }
            );
        services.AddAuthorization();
    }

    private static void ConfigureRabbitHost(IRabbitMqBusFactoryConfigurator rabbit, IConfiguration configuration)
    {
        rabbit.Host(configuration["RabbitMq:Host"] ?? "localhost", "/", host =>
        {
            host.Username(configuration["RabbitMq:Username"] ?? "guest");
            host.Password(configuration["RabbitMq:Password"] ?? "guest");
        });
    }

    private static void AddSwaggerWithBearerAuthentication(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Informe somente o token JWT. O prefixo Bearer será adicionado automaticamente.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            };

            options.AddSecurityDefinition("Bearer", bearerScheme);
            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement { [bearerScheme] = Array.Empty<string>() }
            );
        });
    }
}
