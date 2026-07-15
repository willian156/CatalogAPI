using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class ListPurchasesQuery : IRequest<IReadOnlyList<PurchaseDto>>
{
    public Guid UserId { get; set; }
}
