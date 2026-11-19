namespace CatalogAPI.Infrastructure.Reviews;

public interface IGameReviewRepository
{
    Task<IReadOnlyList<GameReview>> ListAsync(Guid gameId, CancellationToken ct);
    Task<GameReview> AddAsync(GameReview review, CancellationToken ct);
}
