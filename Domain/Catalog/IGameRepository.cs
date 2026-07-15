namespace CatalogAPI.Domain.Catalog;

public interface IGameRepository
{
    Task<Game?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Game>> ListAsync(CancellationToken ct);
    void Add(Game game);
    void Remove(Game game);
    Task SaveAsync(CancellationToken ct);
}
