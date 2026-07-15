using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class GetPurchaseQuery : IRequest<PurchaseDto?>
{
    public Guid Id { get; set; }
}
