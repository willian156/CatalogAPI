using System.Net;
using System.Text.Json;
using CatalogAPI.Application.Search;
using CatalogAPI.Domain.Catalog;
using CatalogAPI.Infrastructure.Persistence;
using CatalogAPI.Infrastructure.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace CatalogAPI.Tests;

public sealed class SearchTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IndexCreationSupportsServerlessAndHosted(bool serverless)
    {
        var created = false;
        using var handler = new StubHandler(async request =>
        {
            if (request.Method == HttpMethod.Head)
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            Assert.Equal(HttpMethod.Put, request.Method);
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            var settings = json.RootElement.GetProperty("settings");
            Assert.Equal(!serverless, settings.TryGetProperty("number_of_shards", out _));
            Assert.Equal(!serverless, settings.TryGetProperty("number_of_replicas", out _));
            Assert.Equal("keyword", json.RootElement.GetProperty("mappings").GetProperty("properties").GetProperty("id").GetProperty("type").GetString());
            created = true;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:9200/") };
        await new ElasticsearchGameSearch(client, Options.Create(new SearchOptions { Serverless = serverless })).EnsureIndexAsync(default);
        Assert.True(created);
    }

    [Fact]
    public async Task SearchUsesFuzzinessAndRelevanceAndPreservesScores()
    {
        var id = Guid.NewGuid();
        using var handler = new StubHandler(async request =>
        {
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Assert.Equal("AUTO", json.RootElement.GetProperty("query").GetProperty("multi_match").GetProperty("fuzziness").GetString());
            Assert.Equal("desc", json.RootElement.GetProperty("sort")[0].GetProperty("_score").GetString());
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new
            {
                hits = new { total = new { value = 1 }, hits = new[] { new { _score = 2.4, _source = new { id, title = "Portal", description = "Puzzle", genre = "Puzzle", publisher = "Valve", price = 10 } } } }
            })) };
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:9200/") };
        var result = await new ElasticsearchGameSearch(client, Options.Create(new SearchOptions())).SearchAsync("portl", 1, 20, default);
        Assert.Equal(id, Assert.Single(result.Items).Id);
        Assert.Equal(2.4, result.Items[0].Score);
    }

    [Fact]
    public async Task SearchFailureIsReportedInsteadOfFallingBackToSql()
    {
        using var handler = new StubHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:9200/") };
        var search = new ElasticsearchGameSearch(client, Options.Create(new SearchOptions()));
        await Assert.ThrowsAsync<SearchUnavailableException>(() => search.SearchAsync("portal", 1, 20, default));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => search.SearchAsync("portal", 501, 20, default));
    }

    [Fact]
    public async Task CreatingUpdatingAndDeletingGameEachQueuesDurableChange()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var database = new CatalogDbContext(options);
        var game = Game.Create("Portal", "Puzzle", "Puzzle", "Valve", 10);
        database.Games.Add(game);
        await database.SaveChangesAsync();
        Assert.Equal(game.Id, (await database.SearchChanges.SingleAsync()).GameId);
        var saved = await database.Games.SingleAsync();
        saved.Update("Portal 2", "Puzzle", "Puzzle", "Valve", 20);
        await database.SaveChangesAsync();
        database.Games.Remove(await database.Games.SingleAsync());
        await database.SaveChangesAsync();
        Assert.Equal(3, await database.SearchChanges.CountAsync());
    }

    private sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request);
    }
}
