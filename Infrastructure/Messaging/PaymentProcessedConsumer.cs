using CatalogAPI.Application.Abstractions;
using CatalogAPI.Application.Catalog;
using CatalogAPI.Contracts;
using MassTransit;

namespace CatalogAPI.Infrastructure.Messaging;

public class PaymentProcessedConsumer(ICommandHandler<ProcessPaymentCommand, bool> handler)
    : IConsumer<PaymentProcessedEvent>
{
    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context) =>
        await handler.Handle(
            new ProcessPaymentCommand { Event = context.Message },
            context.CancellationToken
        );
}
