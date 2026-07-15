using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class ProcessPaymentCommand : IRequest<bool>
{
    public PaymentProcessedEvent Event { get; set; } = new();
}
