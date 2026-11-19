using CatalogAPI.Application.Catalog;
using Prometheus;
using StackExchange.Redis;

namespace CatalogAPI.Infrastructure.Cache;

public sealed class ResilientGameCache(RedisGameCache cache, ILogger<ResilientGameCache> logger) : IGameCache
{
    private static readonly Counter Reads = Metrics.CreateCounter("fcg_cache_reads_total",
        "Leituras do cache do catálogo.", new CounterConfiguration { LabelNames = ["result"] });
    public Task<GameDto?> GetAsync(Guid id, CancellationToken ct) => ReadAsync(() => cache.GetAsync(id, ct));
    public Task<IReadOnlyList<GameDto>?> GetListAsync(CancellationToken ct) => ReadAsync(() => cache.GetListAsync(ct));
    public Task SetAsync(GameDto game, CancellationToken ct) => WriteAsync(() => cache.SetAsync(game, ct));
    public Task SetListAsync(IReadOnlyList<GameDto> games, CancellationToken ct) => WriteAsync(() => cache.SetListAsync(games, ct));
    public Task InvalidateAsync(Guid id, CancellationToken ct) => WriteAsync(() => cache.InvalidateAsync(id, ct));
    public Task InvalidateListAsync(CancellationToken ct) => WriteAsync(() => cache.InvalidateListAsync(ct));

    private async Task<T?> ReadAsync<T>(Func<Task<T?>> action) where T : class
    {
        try
        {
            var value = await action();
            Reads.WithLabels(value is null ? "miss" : "hit").Inc();
            return value;
        }
        catch (Exception exception) when (exception is RedisException or TimeoutException)
        {
            Reads.WithLabels("error").Inc();
            logger.LogWarning("Cache indisponível; consulta seguirá para o banco de dados.");
            return null;
        }
    }
    private async Task WriteAsync(Func<Task> action)
    {
        try { await action(); }
        catch (Exception exception) when (exception is RedisException or TimeoutException)
        {
            logger.LogWarning("Cache indisponível; valores antigos expiram pelo TTL configurado.");
        }
    }
}
