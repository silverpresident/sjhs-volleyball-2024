using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Hubs;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Controllers
{
    public class MatchesController : Controller
    {
        private readonly IMatchService _matchService;
        private readonly IHubContext<MatchHub> _matchHub;
        private readonly IActiveTournamentService _activeTournamentService;
        private readonly ILogger<MatchesController> _logger;

        public MatchesController(ILogger<MatchesController> logger, IActiveTournamentService activeTournamentService, IMatchService matchService, IHubContext<MatchHub> matchHub)
        {
            _matchService = matchService;
            _matchHub = matchHub;
            _activeTournamentService = activeTournamentService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(Guid? divisionId, string? groupName, Guid? roundId, Guid? teamId)
        {

            var matches = await _activeTournamentService.GetMatchesAsync(divisionId, roundId, groupName, teamId);
             
            return View(matches.ToList());
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var match = await _matchService.GetMatchAsync(id);
            if (match == null)
            {
                return NotFound();
            }
            return View(match);
        }
    }
}
