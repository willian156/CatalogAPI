using System.ComponentModel.DataAnnotations;

namespace CatalogAPI.Infrastructure.Search;

public sealed class SearchOptions
{
    [Required, Url]
    public string Endpoint { get; set; } = "http://localhost:9200";
    [Required, RegularExpression("^[a-z][a-z0-9_-]*$")]
    public string Index { get; set; } = "fcg-games";
    public string? ApiKey { get; set; }
    public bool Serverless { get; set; }
    [Range(1, 10)]
    public int PollIntervalSeconds { get; set; } = 5;
    [Range(1, 500)]
    public int BatchSize { get; set; } = 100;
}
