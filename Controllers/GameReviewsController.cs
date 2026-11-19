using CatalogAPI.Domain.Catalog;
using CatalogAPI.Infrastructure.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("games/{gameId:guid}/reviews")]
public sealed class GameReviewsController(IGameRepository games, IGameReviewRepository reviews) : ControllerBase
{
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> List(Guid gameId, CancellationToken ct)
    {
        if (await games.GetAsync(gameId, ct) is null) return NotFound();
        return Ok(await reviews.ListAsync(gameId, ct));
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Create(Guid gameId, CreateGameReviewRequest request, CancellationToken ct)
    {
        if (await games.GetAsync(gameId, ct) is null) return NotFound();
        if (request.Rating is < 1 or > 5)
            return ValidationProblem("A nota deve estar entre 1 e 5.");
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var review = new GameReview
        {
            GameId = gameId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment?.Trim() ?? string.Empty,
        };
        await reviews.AddAsync(review, ct);
        return CreatedAtAction(nameof(List), new { gameId }, review);
    }
}
