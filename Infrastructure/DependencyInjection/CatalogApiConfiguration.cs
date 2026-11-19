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
using MongoDB.Driver;
using Prometheus;
using CatalogAPI.Infrastructure.Cache;
using CatalogAPI.Infrastructure.Reviews;
using CatalogAPI.Infrastructure.Search;
using CatalogAPI.Application.Search;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace CatalogAPI.Infrastructure.DependencyInjection;

public static class CatalogApiConfiguration
{
    public static IServiceCollection AddCatalogApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCatalogApplication();
        services.AddCatalogPersistence(configuration);
        services.AddCatalogPerformance(configuration);
        services.AddOptions<SearchOptions>().Bind(configuration.GetSection("Search"))
            .ValidateDataAnnotations()
            .Validate(options => string.IsNullOrEmpty(options.ApiKey) ||
                Uri.TryCreate(options.Endpoint, UriKind.Absolute, out var endpoint) && endpoint.Scheme == Uri.UriSchemeHttps,
                "Search:ApiKey exige um endpoint HTTPS.")
            .ValidateOnStart();
        services.AddHttpClient<ElasticsearchGameSearch>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<SearchOptions>>().Value;
            client.BaseAddress = new Uri(options.Endpoint.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(5);
            if (!string.IsNullOrWhiteSpace(options.ApiKey))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ApiKey", options.ApiKey);
        });
        services.AddTransient<IGameSearch>(provider => provider.GetRequiredService<ElasticsearchGameSearch>());
        services.AddHostedService<SearchIndexWorker>();
        services.AddScoped<IReindexGames, ReindexGames>();
        services.AddCatalogMessaging(configuration);
        services.AddCatalogAuthentication(configuration);
        services.AddApiCors(configuration);
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerWithBearerAuthentication();
        return services;
    }

    public static WebApplication UseCatalogApi(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseCors("Frontend");
        app.UseHttpMetrics(options => options.ReduceStatusCodeCardinality());
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapMetrics();
        return app;
    }

    private static void AddApiCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options =>
            options.AddPolicy("Frontend", policy =>
                policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()
            )
        );
    }

    public static async Task InitializeCatalogDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await DatabaseInitialization.RunAsync(database, async () =>
        {
            await database.Database.EnsureCreatedAsync();
            await SearchSchema.InitializeAsync(database);
        });
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

    private static void AddCatalogPerformance(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379");
        services.AddSingleton<RedisGameCache>();
        services.AddSingleton<IGameCache, ResilientGameCache>();

        services.AddSingleton<IMongoClient>(_ => new MongoClient(
            configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017"));
        services.AddSingleton<IGameReviewRepository, MongoGameReviewRepository>();
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
        var key = configuration["Jwt:Key"] ?? string.Empty;
        if (Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("Configure Jwt:Key com pelo menos 32 bytes por um provedor de segredos.");
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
            host.Username(configuration["RabbitMq:Username"] ?? throw new InvalidOperationException("Configure RabbitMq:Username."));
            host.Password(configuration["RabbitMq:Password"] ?? throw new InvalidOperationException("Configure RabbitMq:Password."));
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
