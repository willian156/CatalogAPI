using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class CreateGameHandler(IGameRepository r) : IRequestHandler<CreateGameCommand, GameDto>
{
    public async Task<GameDto> Handle(CreateGameCommand c, CancellationToken ct)
    {
        var d = c.Data;
        var x = Game.Create(d.Title, d.Description, d.Genre, d.Publisher, d.Price);
        r.Add(x);
        await r.SaveAsync(ct);
        return Mapping.ToDto(x);
    }
}
