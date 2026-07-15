namespace CatalogAPI.Domain.Catalog;

public interface IGameRepository
{
    Task<Game?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Game>> ListAsync(CancellationToken ct);
    void Add(Game game);
    void Remove(Game game);
    Task SaveAsync(CancellationToken ct);
}

public interface IPurchaseRepository
{
    Task<Purchase?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Purchase>> ListByUserAsync(Guid userId, CancellationToken ct);
    Task<bool> HasLibraryItemAsync(Guid userId, Guid gameId, CancellationToken ct);
    Task<IReadOnlyList<Game>> GetLibraryAsync(Guid userId, CancellationToken ct);
    void Add(Purchase purchase);
    void AddLibraryItem(LibraryItem item);
    Task SaveAsync(CancellationToken ct);
}
