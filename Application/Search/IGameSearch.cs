namespace CatalogAPI.Application.Search;

public interface IGameSearch
{
    Task<SearchResult> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken);
}

public sealed record SearchHit(Guid Id, string Title, string Description, string Genre,
    string Publisher, decimal Price, double Score);

public sealed record SearchResult(long Total, int Page, int PageSize, IReadOnlyList<SearchHit> Items);

public sealed class SearchUnavailableException(Exception innerException)
    : Exception("O serviço de busca está temporariamente indisponível.", innerException);
