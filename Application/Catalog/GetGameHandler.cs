using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class GetGameHandler(IGameRepository r) : IRequestHandler<GetGameQuery, GameDto?>
{
    public async Task<GameDto?> Handle(GetGameQuery q, CancellationToken ct)
    {
        var x = await r.GetAsync(q.Id, ct);
        return x is null ? null : Mapping.ToDto(x);
    }
}
