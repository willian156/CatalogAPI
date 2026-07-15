using System.Security.Claims;
using CatalogAPI.Application.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("games")]
public class GamesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(await sender.Send(new ListGamesQuery(), ct));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var game = await sender.Send(new GetGameQuery { Id = id }, ct);
        return game is null ? NotFound() : Ok(game);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(GameWriteDto dto, CancellationToken ct)
    {
        var game = await sender.Send(new CreateGameCommand { Data = dto }, ct);
        return CreatedAtAction(nameof(Get), new { id = game.Id }, game);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, GameWriteDto dto, CancellationToken ct)
    {
        var game = await sender.Send(new UpdateGameCommand { Id = id, Data = dto }, ct);
        return game is null ? NotFound() : Ok(game);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await sender.Send(new DeleteGameCommand { Id = id }, ct) ? NoContent() : NotFound();
}

[ApiController]
[Route("purchases")]
[Authorize]
public class PurchasesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Place(PlacePurchaseDto dto, CancellationToken ct)
    {
        if (!CanAccess(dto.UserId))
            return Forbid();

        var purchase = await sender.Send(new PlacePurchaseCommand { Data = dto }, ct);
        return purchase is null
            ? NotFound(new { message = "Jogo não encontrado." })
            : AcceptedAtAction(nameof(Get), new { id = purchase.Id }, purchase);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var purchase = await sender.Send(new GetPurchaseQuery { Id = id }, ct);
        return purchase is null ? NotFound() : Ok(purchase);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> List(Guid userId, CancellationToken ct) =>
        CanAccess(userId)
            ? Ok(await sender.Send(new ListPurchasesQuery { UserId = userId }, ct))
            : Forbid();

    [HttpGet("/users/{userId:guid}/library")]
    public async Task<IActionResult> Library(Guid userId, CancellationToken ct) =>
        CanAccess(userId)
            ? Ok(await sender.Send(new GetLibraryQuery { UserId = userId }, ct))
            : Forbid();

    private bool CanAccess(Guid id) =>
        User.IsInRole("Admin")
        || User.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString()
        || User.FindFirstValue("sub") == id.ToString();
}
