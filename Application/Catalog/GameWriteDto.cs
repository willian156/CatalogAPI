namespace CatalogAPI.Application.Catalog;

public class GameWriteDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
