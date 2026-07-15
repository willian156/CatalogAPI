using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class ListGamesHandler(IGameRepository r) : IRequestHandler<ListGamesQuery, IReadOnlyList<GameDto>>
{
    public async Task<IReadOnlyList<GameDto>> Handle(ListGamesQuery q, CancellationToken ct) => (await r.ListAsync(ct)).Select(Mapping.ToDto).ToList();
}
