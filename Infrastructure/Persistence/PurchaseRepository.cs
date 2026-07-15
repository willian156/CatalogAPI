using CatalogAPI.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Persistence;

public class PurchaseRepository(CatalogDbContext db) : IPurchaseRepository
{
    public Task<Purchase?> GetAsync(Guid id, CancellationToken ct) => db.Purchases.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<Purchase>> ListByUserAsync(Guid id, CancellationToken ct) => await db.Purchases.AsNoTracking().Where(x => x.UserId == id).OrderByDescending(x => x.PlacedAt).ToListAsync(ct);
    public Task<bool> HasLibraryItemAsync(Guid u, Guid g, CancellationToken ct) => db.Library.AnyAsync(x => x.UserId == u && x.GameId == g, ct);
    public async Task<IReadOnlyList<Game>> GetLibraryAsync(Guid u, CancellationToken ct) => await (
        from l in db.Library
        join g in db.Games on l.GameId equals g.Id
        where l.UserId == u
        orderby l.AddedAt descending
        select g).AsNoTracking().ToListAsync(ct);
    public void Add(Purchase x) => db.Purchases.Add(x);
    public void AddLibraryItem(LibraryItem x) => db.Library.Add(x);
    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
