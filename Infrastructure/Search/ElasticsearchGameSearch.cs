using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CatalogAPI.Application.Search;
using Microsoft.Extensions.Options;

namespace CatalogAPI.Infrastructure.Search;

public sealed class ElasticsearchGameSearch(HttpClient client, IOptions<SearchOptions> options) : IGameSearch
{
    private readonly string index = options.Value.Index;

    public async Task<SearchResult> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length > 200)
            throw new ArgumentException("Informe uma busca entre 1 e 200 caracteres.", nameof(query));
        if (page < 1 || pageSize is < 1 or > 100 || (long)page * pageSize > 10_000)
            throw new ArgumentOutOfRangeException(nameof(page), "Paginação fora dos limites permitidos.");

        try
        {
            using var response = await client.PostAsJsonAsync($"{index}/_search", new
            {
                from = (page - 1) * pageSize,
                size = pageSize,
                track_total_hits = true,
                query = new { multi_match = new { query = query.Trim(), fields = new[] { "title^3", "description", "genre^2", "publisher" }, fuzziness = "AUTO" } },
                sort = new object[] { new { _score = "desc" }, new { id = "asc" } }
            }, cancellationToken);
            response.EnsureSuccessStatusCode();
            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var hits = json.RootElement.GetProperty("hits");
            var items = hits.GetProperty("hits").EnumerateArray().Select(hit =>
            {
                var document = hit.GetProperty("_source").Deserialize<GameSearchDocument>(new JsonSerializerOptions(JsonSerializerDefaults.Web))
                    ?? throw new JsonException("Documento de busca inválido.");
                return new SearchHit(document.Id, document.Title, document.Description, document.Genre,
                    document.Publisher, document.Price, hit.GetProperty("_score").GetDouble());
            }).ToArray();
            return new SearchResult(hits.GetProperty("total").GetProperty("value").GetInt64(), page, pageSize, items);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException ||
            exception is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw new SearchUnavailableException(exception);
        }
    }

    public async Task EnsureIndexAsync(CancellationToken cancellationToken)
    {
        using var exists = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, index), cancellationToken);
        if (exists.IsSuccessStatusCode) return;
        if (exists.StatusCode != HttpStatusCode.NotFound) exists.EnsureSuccessStatusCode();
        using var response = await client.PutAsJsonAsync(index, new
        {
            settings = options.Value.Serverless
                ? new Dictionary<string, int>()
                : new Dictionary<string, int> { ["number_of_shards"] = 1, ["number_of_replicas"] = 0 },
            mappings = new { properties = new
            {
                id = new { type = "keyword" }, title = new { type = "text" },
                description = new { type = "text" }, genre = new { type = "text" },
                publisher = new { type = "text" }, price = new { type = "scaled_float", scaling_factor = 100 }
            } }
        }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SynchronizeAsync(Guid gameId, GameSearchDocument? document, CancellationToken cancellationToken)
    {
        var path = $"{index}/_doc/{gameId}";
        using var response = document is null
            ? await client.DeleteAsync(path, cancellationToken)
            : await client.PutAsJsonAsync(path, document, cancellationToken);
        if (document is null && response.StatusCode == HttpStatusCode.NotFound) return;
        response.EnsureSuccessStatusCode();
    }
}
