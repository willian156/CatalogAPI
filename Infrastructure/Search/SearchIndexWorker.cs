using CatalogAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CatalogAPI.Infrastructure.Search;

public sealed class SearchIndexWorker(IServiceScopeFactory scopeFactory,
    IOptions<SearchOptions> options, ILogger<SearchIndexWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.PollIntervalSeconds));
        do
        {
            try
            {
                await SynchronizeBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception)
            {
                // No payloads or credentials in logs. Pending changes remain durable for retry.
                logger.LogWarning("Indexação adiada ({ErrorType}); nova tentativa no próximo ciclo.", exception.GetType().Name);
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task SynchronizeBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var search = scope.ServiceProvider.GetRequiredService<ElasticsearchGameSearch>();
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        // Serialize workers across replicas. Read current state, so delayed events cannot restore stale data.
        var ownsLock = await database.Database.SqlQueryRaw<bool>(
            "SELECT pg_try_advisory_xact_lock(748391) AS \"Value\"").SingleAsync(cancellationToken);
        if (!ownsLock) return;

        await search.EnsureIndexAsync(cancellationToken);
        var changes = await database.SearchChanges.OrderBy(change => change.Id)
            .Take(options.Value.BatchSize).ToListAsync(cancellationToken);
        foreach (var gameId in changes.Select(change => change.GameId).Distinct())
        {
            var game = await database.Games.AsNoTracking().SingleOrDefaultAsync(game => game.Id == gameId, cancellationToken);
            await search.SynchronizeAsync(gameId, game is null ? null : GameSearchDocument.From(game), cancellationToken);
        }
        database.SearchChanges.RemoveRange(changes);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        if (changes.Count > 0) logger.LogInformation("Indexadas {Count} alterações de jogos.", changes.Count);
    }
}
