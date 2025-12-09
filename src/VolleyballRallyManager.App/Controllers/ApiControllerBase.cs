using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Hubs;

namespace VolleyballRallyManager.App.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected readonly ApplicationDbContext Context;
    protected readonly IHubContext<TournamentHub> _hubContext;
    protected readonly ILogger _logger;

    protected ApiControllerBase(
        ApplicationDbContext context,
        IHubContext<TournamentHub> hubContext,
        ILogger logger)
    {
        Context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected string CurrentUserId => User?.Identity?.Name ?? "system";

    protected ActionResult HandleError(Exception ex)
    {
        _logger.LogError(ex, "An error occurred while processing the request");

        if (ex is KeyNotFoundException)
            return NotFound(ex.Message);

        return StatusCode(500, new { message = "An error occurred while processing your request." });
    }
}