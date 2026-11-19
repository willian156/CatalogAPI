using System.ComponentModel.DataAnnotations;
using CatalogAPI.Application.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("search")]
public sealed class SearchController(IGameSearch search) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromQuery, Required, StringLength(200, MinimumLength = 1)] string q,
        [FromQuery, Range(1, 10000)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q) || (long)page * pageSize > 10_000)
            return BadRequest(new ProblemDetails { Title = "Busca ou paginação inválida." });
        try
        {
            return Ok(await search.SearchAsync(q, page, pageSize, cancellationToken));
        }
        catch (SearchUnavailableException)
        {
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Busca temporariamente indisponível. Tente novamente em instantes.");
        }
    }
}
