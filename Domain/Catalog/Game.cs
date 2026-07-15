namespace CatalogAPI.Domain.Catalog;

public class Game
{
    private Game()
    {
    }

    private Game(Guid id, string title, string description, string genre, string publisher, decimal price)
    {
        Id = id;
        Update(title, description, genre, publisher, price);
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Genre { get; private set; } = string.Empty;
    public string Publisher { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Game Create(string title, string description, string genre, string publisher, decimal price) => new(Guid.NewGuid(), title, description, genre, publisher, price);
    public void Update(string title, string description, string genre, string publisher, decimal price)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Título obrigatório.");
        if (price < 0)
            throw new DomainException("Preço não pode ser negativo.");
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Genre = genre?.Trim() ?? string.Empty;
        Publisher = publisher?.Trim() ?? string.Empty;
        Price = price;
    }
}
