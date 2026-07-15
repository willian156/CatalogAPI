namespace CatalogAPI.Domain.Catalog;

public class Purchase
{
    private Purchase()
    {
    }

    private Purchase(Guid id, Guid userId, Guid gameId, decimal price)
    {
        Id = id;
        UserId = userId;
        GameId = gameId;
        Price = price;
        Status = PurchaseStatus.Pending;
        PlacedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public decimal Price { get; private set; }
    public PurchaseStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset PlacedAt { get; private set; }

    public static Purchase Place(Guid userId, Game game) => new(Guid.NewGuid(), userId, game.Id, game.Price);
    public void Process(bool approved, string? reason)
    {
        if (Status != PurchaseStatus.Pending)
            return;
        Status = approved ? PurchaseStatus.Approved : PurchaseStatus.Rejected;
        FailureReason = reason;
    }
}
