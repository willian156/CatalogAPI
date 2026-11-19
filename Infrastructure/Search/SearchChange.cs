using CatalogAPI.Domain.Catalog;

namespace CatalogAPI.Infrastructure.Search;

public sealed class SearchChange
{
    public long Id { get; set; }
    public Guid GameId { get; set; }
}

public sealed record GameSearchDocument(Guid Id, string Title, string Description, string Genre,
    string Publisher, decimal Price)
{
    public static GameSearchDocument From(Game game) =>
        new(game.Id, game.Title, game.Description, game.Genre, game.Publisher, game.Price);
}
