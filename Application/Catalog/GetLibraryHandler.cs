using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class GetLibraryHandler(IPurchaseRepository r) : IRequestHandler<GetLibraryQuery, IReadOnlyList<GameDto>>
{
    public async Task<IReadOnlyList<GameDto>> Handle(GetLibraryQuery q, CancellationToken ct) => (await r.GetLibraryAsync(q.UserId, ct)).Select(Mapping.ToDto).ToList();
}
