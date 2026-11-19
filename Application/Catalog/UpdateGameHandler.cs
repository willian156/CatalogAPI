using CatalogAPI.Domain.Catalog;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class UpdateGameHandler(IGameRepository repository, IGameCache cache) : IRequestHandler<UpdateGameCommand, GameDto?>
{
    public async Task<GameDto?> Handle(UpdateGameCommand command, CancellationToken cancellationToken)
    {
        var game = await repository.GetAsync(command.Id, cancellationToken);
        if (game is null)
            return null;
        var data = command.Data;
        game.Update(data.Title, data.Description, data.Genre, data.Publisher, data.Price);
        await repository.SaveAsync(cancellationToken);
        await cache.InvalidateAsync(game.Id, cancellationToken);
        var dto = Mapping.ToDto(game);
        await cache.SetAsync(dto, cancellationToken);
        return dto;
    }
}
