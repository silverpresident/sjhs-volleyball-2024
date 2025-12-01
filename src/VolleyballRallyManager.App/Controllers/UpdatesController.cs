using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Controllers;

public class UpdatesController : Controller
{
    private readonly IMatchService _matchService;
    private readonly ILogger<UpdatesController> _logger;

    public UpdatesController(IMatchService matchService, ILogger<UpdatesController> logger)
    {
        _matchService = matchService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var updates = await _matchService.GetRecentMatchUpdatesAsync(25);
            return View(updates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent match updates");
            return View("Error");
        }
    }
}
