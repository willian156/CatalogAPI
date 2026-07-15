using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;

namespace CatalogAPI.Application.Catalog;

public class CreateGameCommand : IRequest<GameDto>
{
    public GameWriteDto Data { get; set; } = new();
}

public class UpdateGameCommand : IRequest<GameDto?>
{
    public Guid Id { get; set; }
    public GameWriteDto Data { get; set; } = new();
}

public class DeleteGameCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class PlacePurchaseCommand : IRequest<PurchaseDto?>
{
    public PlacePurchaseDto Data { get; set; } = new();
}

public class ProcessPaymentCommand : IRequest<bool>
{
    public PaymentProcessedEvent Event { get; set; } = new();
}

public class GetGameQuery : IRequest<GameDto?>
{
    public Guid Id { get; set; }
}

public class ListGamesQuery : IRequest<IReadOnlyList<GameDto>> { }

public class GetPurchaseQuery : IRequest<PurchaseDto?>
{
    public Guid Id { get; set; }
}

public class ListPurchasesQuery : IRequest<IReadOnlyList<PurchaseDto>>
{
    public Guid UserId { get; set; }
}

public class GetLibraryQuery : IRequest<IReadOnlyList<GameDto>>
{
    public Guid UserId { get; set; }
}

public static class Mapping
{
    public static GameDto ToDto(Game x) =>
        new()
        {
            Id = x.Id,
            Title = x.Title,
            Description = x.Description,
            Genre = x.Genre,
            Publisher = x.Publisher,
            Price = x.Price,
            CreatedAt = x.CreatedAt,
        };

    public static PurchaseDto ToDto(Purchase x) =>
        new()
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

public class CreateGameHandler(IGameRepository r) : IRequestHandler<CreateGameCommand, GameDto>
{
    public async Task<GameDto> Handle(CreateGameCommand c, CancellationToken ct)
    {
        var d = c.Data;
        var x = Game.Create(d.Title, d.Description, d.Genre, d.Publisher, d.Price);
        r.Add(x);
        await r.SaveAsync(ct);
        return Mapping.ToDto(x);
    }
}

public class UpdateGameHandler(IGameRepository r) : IRequestHandler<UpdateGameCommand, GameDto?>
{
    public async Task<GameDto?> Handle(UpdateGameCommand c, CancellationToken ct)
    {
        var x = await r.GetAsync(c.Id, ct);
        if (x is null)
            return null;
        var d = c.Data;
        x.Update(d.Title, d.Description, d.Genre, d.Publisher, d.Price);
        await r.SaveAsync(ct);
        return Mapping.ToDto(x);
    }
}

public class DeleteGameHandler(IGameRepository r) : IRequestHandler<DeleteGameCommand, bool>
{
    public async Task<bool> Handle(DeleteGameCommand c, CancellationToken ct)
    {
        var x = await r.GetAsync(c.Id, ct);
        if (x is null)
            return false;
        r.Remove(x);
        await r.SaveAsync(ct);
        return true;
    }
}

public class PlacePurchaseHandler(
    IGameRepository games,
    IPurchaseRepository purchases,
    IPublishEndpoint bus
) : IRequestHandler<PlacePurchaseCommand, PurchaseDto?>
{
    public async Task<PurchaseDto?> Handle(PlacePurchaseCommand c, CancellationToken ct)
    {
        var game = await games.GetAsync(c.Data.GameId, ct);
        if (game is null)
            return null;
        var x = Purchase.Place(c.Data.UserId, game);
        purchases.Add(x);
        await purchases.SaveAsync(ct);
        await bus.Publish(
            new OrderPlacedEvent
            {
                OrderId = x.Id,
                UserId = x.UserId,
                GameId = x.GameId,
                Price = x.Price,
                PlacedAt = x.PlacedAt,
            },
            ct
        );
        return Mapping.ToDto(x);
    }
}

public class ProcessPaymentHandler(IPurchaseRepository r)
    : IRequestHandler<ProcessPaymentCommand, bool>
{
    public async Task<bool> Handle(ProcessPaymentCommand c, CancellationToken ct)
    {
        var e = c.Event;
        var x = await r.GetAsync(e.OrderId, ct);
        if (x is null)
            return false;
        x.Process(e.Status == PaymentStatus.Approved, e.Reason);
        if (
            e.Status == PaymentStatus.Approved
            && !await r.HasLibraryItemAsync(e.UserId, e.GameId, ct)
        )
            r.AddLibraryItem(new LibraryItem(e.UserId, e.GameId));
        await r.SaveAsync(ct);
        return true;
    }
}

public class GetGameHandler(IGameRepository r) : IRequestHandler<GetGameQuery, GameDto?>
{
    public async Task<GameDto?> Handle(GetGameQuery q, CancellationToken ct)
    {
        var x = await r.GetAsync(q.Id, ct);
        return x is null ? null : Mapping.ToDto(x);
    }
}

public class ListGamesHandler(IGameRepository r)
    : IRequestHandler<ListGamesQuery, IReadOnlyList<GameDto>>
{
    public async Task<IReadOnlyList<GameDto>> Handle(ListGamesQuery q, CancellationToken ct) =>
        (await r.ListAsync(ct)).Select(Mapping.ToDto).ToList();
}

public class GetPurchaseHandler(IPurchaseRepository r)
    : IRequestHandler<GetPurchaseQuery, PurchaseDto?>
{
    public async Task<PurchaseDto?> Handle(GetPurchaseQuery q, CancellationToken ct)
    {
        var x = await r.GetAsync(q.Id, ct);
        return x is null ? null : Mapping.ToDto(x);
    }
}

public class ListPurchasesHandler(IPurchaseRepository r)
    : IRequestHandler<ListPurchasesQuery, IReadOnlyList<PurchaseDto>>
{
    public async Task<IReadOnlyList<PurchaseDto>> Handle(
        ListPurchasesQuery q,
        CancellationToken ct
    ) => (await r.ListByUserAsync(q.UserId, ct)).Select(Mapping.ToDto).ToList();
}

public class GetLibraryHandler(IPurchaseRepository r)
    : IRequestHandler<GetLibraryQuery, IReadOnlyList<GameDto>>
{
    public async Task<IReadOnlyList<GameDto>> Handle(GetLibraryQuery q, CancellationToken ct) =>
        (await r.GetLibraryAsync(q.UserId, ct)).Select(Mapping.ToDto).ToList();
}
