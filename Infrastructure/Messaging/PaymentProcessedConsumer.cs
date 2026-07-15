using CatalogAPI.Application.Catalog;
using CatalogAPI.Contracts;
using MassTransit;

namespace CatalogAPI.Infrastructure.Messaging;

public class PaymentProcessedConsumer(ISender sender)
    : IConsumer<PaymentProcessedEvent>
{
    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context) =>
        await sender.Send(
            new ProcessPaymentCommand { Event = context.Message },
            context.CancellationToken
        );
}
