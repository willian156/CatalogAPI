using System.Security.Claims;
using CatalogAPI.Application.Abstractions;
using CatalogAPI.Application.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[ApiController, Route("games")]
public class GamesController(
    ICommandHandler<CreateGameCommand, GameDto> create,
    ICommandHandler<UpdateGameCommand, GameDto?> update,
    ICommandHandler<DeleteGameCommand, bool> delete,
    IQueryHandler<GetGameQuery, GameDto?> get,
    IQueryHandler<ListGamesQuery, IReadOnlyList<GameDto>> list
) : ControllerBase
{
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await list.Handle(new(), ct));

    [HttpGet("{id:guid}"), AllowAnonymous]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var x = await get.Handle(new() { Id = id }, ct);
        return x is null ? NotFound() : Ok(x);
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(GameWriteDto d, CancellationToken ct)
    {
        var x = await create.Handle(new() { Data = d }, ct);
        return CreatedAtAction(nameof(Get), new { id = x.Id }, x);
    }

    [HttpPut("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, GameWriteDto d, CancellationToken ct)
    {
        var x = await update.Handle(new() { Id = id, Data = d }, ct);
        return x is null ? NotFound() : Ok(x);
    }

    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await delete.Handle(new() { Id = id }, ct) ? NoContent() : NotFound();
}

[ApiController, Route("purchases"), Authorize]
public class PurchasesController(
    ICommandHandler<PlacePurchaseCommand, PurchaseDto?> place,
    IQueryHandler<GetPurchaseQuery, PurchaseDto?> get,
    IQueryHandler<ListPurchasesQuery, IReadOnlyList<PurchaseDto>> list,
    IQueryHandler<GetLibraryQuery, IReadOnlyList<GameDto>> library
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Place(PlacePurchaseDto d, CancellationToken ct)
    {
        if (!CanAccess(d.UserId))
            return Forbid();
        var x = await place.Handle(new() { Data = d }, ct);
        return x is null
            ? NotFound(new { message = "Jogo não encontrado." })
            : AcceptedAtAction(nameof(Get), new { id = x.Id }, x);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var x = await get.Handle(new() { Id = id }, ct);
        return x is null ? NotFound() : Ok(x);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> List(Guid userId, CancellationToken ct) =>
        CanAccess(userId) ? Ok(await list.Handle(new() { UserId = userId }, ct)) : Forbid();

    [HttpGet("/users/{userId:guid}/library")]
    public async Task<IActionResult> Library(Guid userId, CancellationToken ct) =>
        CanAccess(userId) ? Ok(await library.Handle(new() { UserId = userId }, ct)) : Forbid();

    private bool CanAccess(Guid id) =>
        User.IsInRole("Admin")
        || User.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString()
        || User.FindFirstValue("sub") == id.ToString();
}
