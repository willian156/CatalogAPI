using CatalogAPI.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using CatalogAPI.Infrastructure.Search;

namespace CatalogAPI.Infrastructure.Persistence;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> o) : DbContext(o)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<LibraryItem> Library => Set<LibraryItem>();
    public DbSet<SearchChange> SearchChanges => Set<SearchChange>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changedGames = ChangeTracker.Entries<Game>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => entry.Entity.Id).ToArray();
        foreach (var gameId in changedGames)
        {
            if (!ChangeTracker.Entries<SearchChange>().Any(entry =>
                entry.State == EntityState.Added && entry.Entity.GameId == gameId))
                SearchChanges.Add(new SearchChange { GameId = gameId });
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<SearchChange>(entity =>
        {
            entity.ToTable("search_changes");
            entity.HasKey(change => change.Id);
            entity.Property(change => change.Id).UseIdentityAlwaysColumn();
        });
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
