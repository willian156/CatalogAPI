using CatalogAPI.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Persistence;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> o) : DbContext(o)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<LibraryItem> Library => Set<LibraryItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Game>(e =>
        {
            e.ToTable("games");
            e.HasKey(x => x.Id);
            e.Property(x => x.Price).HasPrecision(18, 2);
        });
        b.Entity<Purchase>(e =>
        {
            e.ToTable("purchases");
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.Price).HasPrecision(18, 2);
        });
        b.Entity<LibraryItem>(e =>
        {
            e.ToTable("library_items");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.GameId }).IsUnique();
        });
    }
}

public class GameRepository(CatalogDbContext db) : IGameRepository
{
    public Task<Game?> GetAsync(Guid id, CancellationToken ct) =>
        db.Games.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Game>> ListAsync(CancellationToken ct) =>
        await db.Games.AsNoTracking().OrderBy(x => x.Title).ToListAsync(ct);

    public void Add(Game x) => db.Games.Add(x);

    public void Remove(Game x) => db.Games.Remove(x);

    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}

public class PurchaseRepository(CatalogDbContext db) : IPurchaseRepository
{
    public Task<Purchase?> GetAsync(Guid id, CancellationToken ct) =>
        db.Purchases.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Purchase>> ListByUserAsync(Guid id, CancellationToken ct) =>
        await db
            .Purchases.AsNoTracking()
            .Where(x => x.UserId == id)
            .OrderByDescending(x => x.PlacedAt)
            .ToListAsync(ct);

    public Task<bool> HasLibraryItemAsync(Guid u, Guid g, CancellationToken ct) =>
        db.Library.AnyAsync(x => x.UserId == u && x.GameId == g, ct);

    public async Task<IReadOnlyList<Game>> GetLibraryAsync(Guid u, CancellationToken ct) =>
        await (
            from l in db.Library
            join g in db.Games on l.GameId equals g.Id
            where l.UserId == u
            orderby l.AddedAt descending
            select g
        )
            .AsNoTracking()
            .ToListAsync(ct);

    public void Add(Purchase x) => db.Purchases.Add(x);

    public void AddLibraryItem(LibraryItem x) => db.Library.Add(x);

    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
