namespace CatalogAPI.Application.Catalog;

public class GameWriteDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

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

public class PlacePurchaseDto
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
}

public class PurchaseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
}
