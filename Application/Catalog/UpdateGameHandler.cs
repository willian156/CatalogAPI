using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class UpdateGameHandler(IGameRepository r) : IRequestHandler<UpdateGameCommand, GameDto?>
{
    public async Task<GameDto?> Handle(UpdateGameCommand c, CancellationToken ct)
    {
        var x = await r.GetAsync(c.Id, ct);
        if (x is null)
            return null;
        var d = c.Data;
        x.Update(d.Title, d.Description, d.Genre, d.Publisher, d.Price);
        await r.SaveAsync(ct);
        return Mapping.ToDto(x);
    }
}
