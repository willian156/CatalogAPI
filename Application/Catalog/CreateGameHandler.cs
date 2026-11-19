using CatalogAPI.Domain.Catalog;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class CreateGameHandler(IGameRepository repository, IGameCache cache) : IRequestHandler<CreateGameCommand, GameDto>
{
    public async Task<GameDto> Handle(CreateGameCommand command, CancellationToken cancellationToken)
    {
        var data = command.Data;
        var game = Game.Create(data.Title, data.Description, data.Genre, data.Publisher, data.Price);
        repository.Add(game);
        await repository.SaveAsync(cancellationToken);
        await cache.InvalidateListAsync(cancellationToken);
        var dto = Mapping.ToDto(game);
        await cache.SetAsync(dto, cancellationToken);
        return dto;
    }
}
