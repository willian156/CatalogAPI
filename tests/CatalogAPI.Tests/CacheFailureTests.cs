using CatalogAPI.Application.Catalog;
using CatalogAPI.Infrastructure.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Xunit;

namespace CatalogAPI.Tests;

public sealed class CacheFailureTests
{
    [Fact]
    public async Task RedisFailureAllowsDatabaseFallbackAndDoesNotFailCommittedWrites()
    {
        var redis = new RedisGameCache(new UnavailableCache(), new ConfigurationBuilder().Build());
        var cache = new ResilientGameCache(redis, NullLogger<ResilientGameCache>.Instance);
        Assert.Null(await cache.GetAsync(Guid.NewGuid(), default));
        Assert.Null(await cache.GetListAsync(default));
        await cache.SetAsync(new GameDto { Id = Guid.NewGuid() }, default);
        await cache.InvalidateAsync(Guid.NewGuid(), default);
    }

    private sealed class UnavailableCache : IDistributedCache
    {
        private static Exception Offline() => new RedisConnectionException(ConnectionFailureType.UnableToConnect, "offline");
        public byte[]? Get(string key) => throw Offline();
        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromException<byte[]?>(Offline());
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => throw Offline();
        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => Task.FromException(Offline());
        public void Refresh(string key) => throw Offline();
        public Task RefreshAsync(string key, CancellationToken token = default) => Task.FromException(Offline());
        public void Remove(string key) => throw Offline();
        public Task RemoveAsync(string key, CancellationToken token = default) => Task.FromException(Offline());
    }
}
