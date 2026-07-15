using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class PlacePurchaseCommand : IRequest<PurchaseDto?>
{
    public PlacePurchaseDto Data { get; set; } = new();
}
