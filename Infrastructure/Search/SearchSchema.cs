using CatalogAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Search;

public static class SearchSchema
{
    // Additive upgrade: EnsureCreated alone does not update databases from Phase 3.
    public static Task InitializeAsync(CatalogDbContext database, CancellationToken cancellationToken = default) =>
        database.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS search_changes (
                "Id" bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                "GameId" uuid NOT NULL
            );
            """, cancellationToken);
}
