using MongoDB.Bson.Serialization.Attributes;

namespace CatalogAPI.Infrastructure.Reviews;

public sealed class GameReview
{
    [BsonId]
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid GameId { get; init; }
    public Guid UserId { get; init; }
    public int Rating { get; init; }
    public string Comment { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record CreateGameReviewRequest(int Rating, string Comment);
