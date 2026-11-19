using CatalogAPI.Domain.Catalog;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class DeleteGameHandler(IGameRepository repository, IGameCache cache) : IRequestHandler<DeleteGameCommand, bool>
{
    public async Task<bool> Handle(DeleteGameCommand command, CancellationToken cancellationToken)
    {
        var game = await repository.GetAsync(command.Id, cancellationToken);
        if (game is null)
            return false;
        repository.Remove(game);
        await repository.SaveAsync(cancellationToken);
        await cache.InvalidateAsync(game.Id, cancellationToken);
        return true;
    }
}
