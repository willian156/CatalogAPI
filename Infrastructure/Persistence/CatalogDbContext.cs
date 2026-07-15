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
