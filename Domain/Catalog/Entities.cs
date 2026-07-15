namespace CatalogAPI.Domain.Catalog;

public class Game
{
    private Game() { }

    private Game(
        Guid id,
        string title,
        string description,
        string genre,
        string publisher,
        decimal price
    )
    {
        Id = id;
        Update(title, description, genre, publisher, price);
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Genre { get; private set; } = string.Empty;
    public string Publisher { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Game Create(
        string title,
        string description,
        string genre,
        string publisher,
        decimal price
    ) => new(Guid.NewGuid(), title, description, genre, publisher, price);

    public void Update(
        string title,
        string description,
        string genre,
        string publisher,
        decimal price
    )
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Título obrigatório.");
        if (price < 0)
            throw new DomainException("Preço não pode ser negativo.");
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Genre = genre?.Trim() ?? string.Empty;
        Publisher = publisher?.Trim() ?? string.Empty;
        Price = price;
    }
}

public class Purchase
{
    private Purchase() { }

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

    public static Purchase Place(Guid userId, Game game) =>
        new(Guid.NewGuid(), userId, game.Id, game.Price);

    public void Process(bool approved, string? reason)
    {
        if (Status != PurchaseStatus.Pending)
            return;
        Status = approved ? PurchaseStatus.Approved : PurchaseStatus.Rejected;
        FailureReason = reason;
    }
}

public class LibraryItem
{
    private LibraryItem() { }

    public LibraryItem(Guid userId, Guid gameId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        GameId = gameId;
        AddedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }
}

public enum PurchaseStatus
{
    Pending,
    Approved,
    Rejected,
}

public class DomainException : Exception
{
    public DomainException(string message)
        : base(message) { }
}
