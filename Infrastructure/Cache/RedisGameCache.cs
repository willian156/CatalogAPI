using System.Text.Json;
using CatalogAPI.Application.Catalog;
using Microsoft.Extensions.Caching.Distributed;

namespace CatalogAPI.Infrastructure.Cache;

public sealed class RedisGameCache(IDistributedCache cache, IConfiguration configuration) : IGameCache
{
    private const string ListKey = "catalog:games";
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(configuration.GetValue("Cache:GamesTtlSeconds", 60));
    private static string Key(Guid id) => $"catalog:games:{id}";
    private DistributedCacheEntryOptions Options => new() { AbsoluteExpirationRelativeToNow = _ttl };

    public async Task<GameDto?> GetAsync(Guid id, CancellationToken ct) =>
        Deserialize<GameDto>(await cache.GetStringAsync(Key(id), ct));

    public async Task<IReadOnlyList<GameDto>?> GetListAsync(CancellationToken ct) =>
        Deserialize<List<GameDto>>(await cache.GetStringAsync(ListKey, ct));

    public Task SetAsync(GameDto game, CancellationToken ct) =>
        cache.SetStringAsync(Key(game.Id), JsonSerializer.Serialize(game), Options, ct);

    public Task SetListAsync(IReadOnlyList<GameDto> games, CancellationToken ct) =>
        cache.SetStringAsync(ListKey, JsonSerializer.Serialize(games), Options, ct);

    public async Task InvalidateAsync(Guid id, CancellationToken ct)
    {
        await cache.RemoveAsync(Key(id), ct);
        await cache.RemoveAsync(ListKey, ct);
    }

    public Task InvalidateListAsync(CancellationToken ct) => cache.RemoveAsync(ListKey, ct);

    private static T? Deserialize<T>(string? value) =>
        string.IsNullOrWhiteSpace(value) ? default : JsonSerializer.Deserialize<T>(value);
}
