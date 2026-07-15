namespace CatalogAPI.Domain.Catalog;

public class LibraryItem
{
    private LibraryItem()
    {
    }

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
