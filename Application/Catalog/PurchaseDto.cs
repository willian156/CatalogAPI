namespace CatalogAPI.Application.Catalog;

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
