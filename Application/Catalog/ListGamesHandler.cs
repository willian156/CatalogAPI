using CatalogAPI.Domain.Catalog;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class ListGamesHandler(IGameRepository repository, IGameCache cache) : IRequestHandler<ListGamesQuery, IReadOnlyList<GameDto>>
{
    public async Task<IReadOnlyList<GameDto>> Handle(ListGamesQuery query, CancellationToken cancellationToken)
    {
        var cached = await cache.GetListAsync(cancellationToken);
        if (cached is not null) return cached;
        var games = (await repository.ListAsync(cancellationToken)).Select(Mapping.ToDto).ToList();
        await cache.SetListAsync(games, cancellationToken);
        return games;
    }
}
