using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class ListPurchasesHandler(IPurchaseRepository r) : IRequestHandler<ListPurchasesQuery, IReadOnlyList<PurchaseDto>>
{
    public async Task<IReadOnlyList<PurchaseDto>> Handle(ListPurchasesQuery q, CancellationToken ct) => (await r.ListByUserAsync(q.UserId, ct)).Select(Mapping.ToDto).ToList();
}
