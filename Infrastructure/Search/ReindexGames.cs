using CatalogAPI.Application.Search;
using CatalogAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Search;

public sealed class ReindexGames(CatalogDbContext database) : IReindexGames
{
    public Task<int> QueueAllAsync(CancellationToken cancellationToken) => database.Database.ExecuteSqlRawAsync(
        "INSERT INTO search_changes (\"GameId\") SELECT \"Id\" FROM games", cancellationToken);
}
