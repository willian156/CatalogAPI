using CatalogAPI.Domain.Catalog;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class GetGameHandler(IGameRepository repository, IGameCache cache) : IRequestHandler<GetGameQuery, GameDto?>
{
    public async Task<GameDto?> Handle(GetGameQuery query, CancellationToken cancellationToken)
    {
        var cached = await cache.GetAsync(query.Id, cancellationToken);
        if (cached is not null) return cached;
        var game = await repository.GetAsync(query.Id, cancellationToken);
        if (game is null) return null;
        var dto = Mapping.ToDto(game);
        await cache.SetAsync(dto, cancellationToken);
        return dto;
    }
}
