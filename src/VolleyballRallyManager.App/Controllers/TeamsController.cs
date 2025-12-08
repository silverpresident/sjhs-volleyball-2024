using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Hubs;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ITeamService _teamService;
        private readonly IMatchService _matchService;
        private readonly IHubContext<MatchHub> _matchHub;
        private readonly ILogger<TeamsController> _logger;
        private readonly IActiveTournamentService _activeTournamentService;

        public TeamsController(ITeamService teamService, IMatchService matchService, IHubContext<MatchHub> matchHub, ILogger<TeamsController> logger, IActiveTournamentService activeTournamentService)
        {
            _teamService = teamService;
            _matchService = matchService;
            _matchHub = matchHub;
            _logger = logger;
            _activeTournamentService = activeTournamentService;
        }

        public async Task<IActionResult> Index(Guid? divisionId, String? groupName)
        {
            var teams = await _activeTournamentService.GetTournamentTeamsAsync(Guid.Empty);
            if (divisionId.HasValue)
            {
                teams = teams.Where(t => t.DivisionId == divisionId.Value);
                string divisionSubtitle = "";
                var division = await _activeTournamentService.GetDivisionAsync(divisionId.Value);
                if (division != null) {
                    divisionSubtitle = $"{division.Name}";
                }
                if (string.IsNullOrEmpty(groupName) == false)
                {
                    teams = teams.Where(t => t.GroupName == groupName);
                    divisionSubtitle += $" - Group {groupName}";
                }
                ViewBag.Subtitle = divisionSubtitle;
            }

            ViewBag.Divisions = await _activeTournamentService.GetTournamentDivisionsAsync();
            return View(teams.ToList());
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var team = await _activeTournamentService.GetTeamAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            // Fetch matches for this team to display results
            var matches = await _activeTournamentService.GetMatchesAsync(null, null, null, id);
            //await _matchService.GetMatchesByTeamAsync(id);
            ViewBag.Matches = matches;

            return View(team);
        }
    }
}
