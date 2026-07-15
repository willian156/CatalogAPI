namespace CatalogAPI.Application.Catalog;

public class GameDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
