using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Hubs;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Controllers
{
    public class MatchesController : Controller
    {
        private readonly IMatchService _matchService;
        private readonly IRoundService _roundService;
        private readonly IHubContext<TournamentHub> _matchHub;
        private readonly IActiveTournamentService _activeTournamentService;
        private readonly ILogger<MatchesController> _logger;

        public MatchesController(ILogger<MatchesController> logger, IActiveTournamentService activeTournamentService, IMatchService matchService, IHubContext<TournamentHub> matchHub, IRoundService roundService)
        {
            _matchService = matchService;
            _matchHub = matchHub;
            _activeTournamentService = activeTournamentService;
            _logger = logger;
            _roundService = roundService;
        }

        public async Task<IActionResult> Index(Guid? divisionId, string? groupName, Guid? roundId, Guid? teamId)
        {
            string divisionSubtitle = "";
            if (divisionId.HasValue)
            {
                var division = await _activeTournamentService.GetDivisionAsync(divisionId.Value);
                if (division != null)
                {
                    divisionSubtitle = $"{division.Name}";
                }
            }
            if (string.IsNullOrEmpty(groupName) == false)
            {
                divisionSubtitle += $" - Group {groupName}";
            }
            if (roundId.HasValue)
            {
                var round = await _roundService.GetRoundByIdAsync(roundId.Value);
                if (round != null)
                {
                    divisionSubtitle += $" - Round {round.Name}";
                }
            }
            if (teamId.HasValue)
            {
                var team = await _activeTournamentService.GetTeamAsync(teamId.Value);
                if (team != null)
                {
                    divisionSubtitle += $" - Team {team.Team.Name}";
                }
            }

            ViewBag.Subtitle = divisionSubtitle;
            var matches = await _activeTournamentService.GetMatchesAsync(divisionId, roundId, groupName, teamId);
            ViewBag.Divisions = await _activeTournamentService.GetTournamentDivisionsAsync();

            return View(matches.ToList());
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var match = await _matchService.GetMatchAsync(id);
                if (match == null)
                {
                    return NotFound();
                }

                // Fetch MatchSets and Updates
                ViewBag.MatchSets = await _matchService.GetMatchSetsAsync(id);
                ViewBag.MatchUpdates = await _matchService.GetMatchUpdatesAsync(id);

                return View(match);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching match details for match {MatchId}", id);
                return View("Error");
            }
        }
    }
}
