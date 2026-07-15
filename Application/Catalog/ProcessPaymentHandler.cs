using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class ProcessPaymentHandler(IPurchaseRepository r) : IRequestHandler<ProcessPaymentCommand, bool>
{
    public async Task<bool> Handle(ProcessPaymentCommand c, CancellationToken ct)
    {
        var e = c.Event;
        var x = await r.GetAsync(e.OrderId, ct);
        if (x is null)
            return false;
        x.Process(e.Status == PaymentStatus.Approved, e.Reason);
        if (e.Status == PaymentStatus.Approved && !await r.HasLibraryItemAsync(e.UserId, e.GameId, ct))
            r.AddLibraryItem(new LibraryItem(e.UserId, e.GameId));
        await r.SaveAsync(ct);
        return true;
    }
}
