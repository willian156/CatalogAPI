using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public static class Mapping
{
    public static GameDto ToDto(Game x) => new()
    {
        Id = x.Id,
        Title = x.Title,
        Description = x.Description,
        Genre = x.Genre,
        Publisher = x.Publisher,
        Price = x.Price,
        CreatedAt = x.CreatedAt,
    };
    public static PurchaseDto ToDto(Purchase x) => new()
    {
        Id = x.Id,
        UserId = x.UserId,
        GameId = x.GameId,
        Price = x.Price,
        Status = x.Status.ToString(),
        FailureReason = x.FailureReason,
        PlacedAt = x.PlacedAt,
    };
}
