using System.Security.Claims;
using CatalogAPI.Application.Catalog;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("games")]
public class GamesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await sender.Send(new ListGamesQuery(), ct));
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
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => await sender.Send(new DeleteGameCommand { Id = id }, ct) ? NoContent() : NotFound();
}
