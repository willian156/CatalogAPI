using CatalogAPI.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Persistence;

public class GameRepository(CatalogDbContext db) : IGameRepository
{
    public Task<Game?> GetAsync(Guid id, CancellationToken ct) => db.Games.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<Game>> ListAsync(CancellationToken ct) => await db.Games.AsNoTracking().OrderBy(x => x.Title).ToListAsync(ct);
    public void Add(Game x) => db.Games.Add(x);
    public void Remove(Game x) => db.Games.Remove(x);
    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
