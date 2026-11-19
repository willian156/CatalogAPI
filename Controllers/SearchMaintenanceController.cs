using CatalogAPI.Application.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("search/reindex")]
[Authorize(Roles = "Admin")]
public sealed class SearchMaintenanceController(IReindexGames reindex) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Reindex(CancellationToken cancellationToken)
    {
        var count = await reindex.QueueAllAsync(cancellationToken);
        return Accepted(new { queued = count });
    }
}
