namespace CatalogAPI.Application.Catalog;

public interface IGameCache
{
    Task<GameDto?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<GameDto>?> GetListAsync(CancellationToken ct);
    Task SetAsync(GameDto game, CancellationToken ct);
    Task SetListAsync(IReadOnlyList<GameDto> games, CancellationToken ct);
    Task InvalidateAsync(Guid id, CancellationToken ct);
    Task InvalidateListAsync(CancellationToken ct);
}
