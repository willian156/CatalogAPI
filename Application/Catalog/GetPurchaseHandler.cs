using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class GetPurchaseHandler(IPurchaseRepository r) : IRequestHandler<GetPurchaseQuery, PurchaseDto?>
{
    public async Task<PurchaseDto?> Handle(GetPurchaseQuery q, CancellationToken ct)
    {
        var x = await r.GetAsync(q.Id, ct);
        return x is null ? null : Mapping.ToDto(x);
    }
}
