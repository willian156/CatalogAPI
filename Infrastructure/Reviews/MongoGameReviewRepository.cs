using MongoDB.Driver;

namespace CatalogAPI.Infrastructure.Reviews;

public sealed class MongoGameReviewRepository : IGameReviewRepository
{
    private readonly IMongoCollection<GameReview> _reviews;

    public MongoGameReviewRepository(IMongoClient client, IConfiguration configuration)
    {
        var database = client.GetDatabase(configuration["MongoDb:Database"] ?? "fcg_catalog");
        _reviews = database.GetCollection<GameReview>("game_reviews");
    }

    public async Task<IReadOnlyList<GameReview>> ListAsync(Guid gameId, CancellationToken ct) =>
        await _reviews.Find(x => x.GameId == gameId).SortByDescending(x => x.CreatedAt).ToListAsync(ct);

    public async Task<GameReview> AddAsync(GameReview review, CancellationToken ct)
    {
        await _reviews.InsertOneAsync(review, cancellationToken: ct);
        return review;
    }
}
