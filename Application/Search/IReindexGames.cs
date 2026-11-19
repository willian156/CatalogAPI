namespace CatalogAPI.Application.Search;

public interface IReindexGames
{
    Task<int> QueueAllAsync(CancellationToken cancellationToken);
}
