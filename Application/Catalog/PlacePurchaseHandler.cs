using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class PlacePurchaseHandler(IGameRepository games, IPurchaseRepository purchases, IPublishEndpoint bus) : IRequestHandler<PlacePurchaseCommand, PurchaseDto?>
{
    public async Task<PurchaseDto?> Handle(PlacePurchaseCommand c, CancellationToken ct)
    {
        var game = await games.GetAsync(c.Data.GameId, ct);
        if (game is null)
            return null;
        var x = Purchase.Place(c.Data.UserId, game);
        purchases.Add(x);
        await purchases.SaveAsync(ct);
        await bus.Publish(new OrderPlacedEvent { OrderId = x.Id, UserId = x.UserId, GameId = x.GameId, Price = x.Price, PlacedAt = x.PlacedAt, }, ct);
        return Mapping.ToDto(x);
    }
}
