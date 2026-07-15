using CatalogAPI.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCatalogApi(builder.Configuration);

var app = builder.Build();

await app.InitializeCatalogDatabaseAsync();
app.UseCatalogApi();

app.Run();
