using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Fcg.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(new ConcurrentDictionary<Guid, Game>());
builder.Services.AddSingleton(new ConcurrentDictionary<Guid, Purchase>());
builder.Services.AddSingleton(new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, byte>>());
builder.Services.AddMassTransit(x => { x.AddConsumer<PaymentProcessedConsumer>(); x.UsingRabbitMq((context, cfg) => { cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h => { h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest"); h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest"); }); cfg.ReceiveEndpoint(builder.Configuration["RabbitMq:PaymentQueue"] ?? "catalog-payment-results", e => e.ConfigureConsumer<PaymentProcessedConsumer>(context)); }); });
var key = builder.Configuration["Jwt:Key"] ?? "change-this-development-key-with-32-chars";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "FCG", ValidAudience = builder.Configuration["Jwt:Audience"] ?? "FCG", IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) });
builder.Services.AddAuthorization(); builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen();
var app = builder.Build(); app.UseSwagger(); app.UseSwaggerUI(); app.UseAuthentication(); app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/games", (ConcurrentDictionary<Guid, Game> games) => Results.Ok(games.Values));
app.MapGet("/games/{id:guid}", (Guid id, ConcurrentDictionary<Guid, Game> games) => games.TryGetValue(id, out var game) ? Results.Ok(game) : Results.NotFound());
app.MapPost("/games", (GameRequest r, ConcurrentDictionary<Guid, Game> games) => { if (string.IsNullOrWhiteSpace(r.Title) || r.Price < 0) return Results.BadRequest(); var g = new Game(Guid.NewGuid(), r.Title.Trim(), r.Description, r.Genre, r.Publisher, r.Price); games[g.Id] = g; return Results.Created($"/games/{g.Id}", g); }).RequireAuthorization(p => p.RequireRole("Admin"));
app.MapPut("/games/{id:guid}", (Guid id, GameRequest r, ConcurrentDictionary<Guid, Game> games) => { if (!games.ContainsKey(id)) return Results.NotFound(); var g = new Game(id, r.Title, r.Description, r.Genre, r.Publisher, r.Price); games[id] = g; return Results.Ok(g); }).RequireAuthorization(p => p.RequireRole("Admin"));
app.MapDelete("/games/{id:guid}", (Guid id, ConcurrentDictionary<Guid, Game> games) => games.TryRemove(id, out _) ? Results.NoContent() : Results.NotFound()).RequireAuthorization(p => p.RequireRole("Admin"));
app.MapPost("/purchases", async (PurchaseRequest r, ClaimsPrincipal principal, ConcurrentDictionary<Guid, Game> games, ConcurrentDictionary<Guid, Purchase> purchases, IPublishEndpoint bus) => { var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub"); if (!Guid.TryParse(subject, out var authenticated) || authenticated != r.UserId) return Results.Forbid(); if (!games.TryGetValue(r.GameId, out var game)) return Results.NotFound(new { message = "Jogo nao encontrado." }); var order = new Purchase(Guid.NewGuid(), r.UserId, r.GameId, game.Price, "Pending"); purchases[order.Id] = order; await bus.Publish(new OrderPlacedEvent(order.Id, order.UserId, order.GameId, order.Price, DateTimeOffset.UtcNow)); return Results.Accepted($"/purchases/{order.Id}", order); }).RequireAuthorization();
app.MapGet("/purchases/{id:guid}", (Guid id, ConcurrentDictionary<Guid, Purchase> purchases) => purchases.TryGetValue(id, out var p) ? Results.Ok(p) : Results.NotFound()).RequireAuthorization();
app.MapGet("/users/{userId:guid}/library", (Guid userId, ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, byte>> libraries, ConcurrentDictionary<Guid, Game> games) => Results.Ok(libraries.TryGetValue(userId, out var ids) ? ids.Keys.Select(id => games.GetValueOrDefault(id)).Where(x => x is not null) : [])).RequireAuthorization();
app.Run();
public record Game(Guid Id, string Title, string Description, string Genre, string Publisher, decimal Price); public record GameRequest(string Title, string Description, string Genre, string Publisher, decimal Price); public record PurchaseRequest(Guid UserId, Guid GameId); public record Purchase(Guid Id, Guid UserId, Guid GameId, decimal Price, string Status);
public class PaymentProcessedConsumer(ConcurrentDictionary<Guid, Purchase> purchases, ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, byte>> libraries) : IConsumer<PaymentProcessedEvent> { public Task Consume(ConsumeContext<PaymentProcessedEvent> context) { var e = context.Message; if (purchases.TryGetValue(e.OrderId, out var p)) { purchases[e.OrderId] = p with { Status = e.Status.ToString() }; if (e.Status == PaymentStatus.Approved) libraries.GetOrAdd(e.UserId, _ => new())[e.GameId] = 0; } return Task.CompletedTask; } }
namespace Fcg.Contracts { public enum PaymentStatus { Approved, Rejected } public record OrderPlacedEvent(Guid OrderId, Guid UserId, Guid GameId, decimal Price, DateTimeOffset PlacedAt); public record PaymentProcessedEvent(Guid OrderId, Guid UserId, Guid GameId, decimal Price, PaymentStatus Status, string? Reason, DateTimeOffset ProcessedAt); }
