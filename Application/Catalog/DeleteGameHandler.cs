using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class DeleteGameHandler(IGameRepository r) : IRequestHandler<DeleteGameCommand, bool>
{
    public async Task<bool> Handle(DeleteGameCommand c, CancellationToken ct)
    {
        var x = await r.GetAsync(c.Id, ct);
        if (x is null)
            return false;
        r.Remove(x);
        await r.SaveAsync(ct);
        return true;
    }
}
