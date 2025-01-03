using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Services;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Hubs;

namespace VolleyballRallyManager.App.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ITeamService _teamService;
        private readonly IMatchService _matchService;
        private readonly IHubContext<MatchHub> _matchHub;

        public TeamsController(ITeamService teamService, IMatchService matchService, IHubContext<MatchHub> matchHub)
        {
            _teamService = teamService;
            _matchService = matchService;
            _matchHub = matchHub;
        }

        public async Task<IActionResult> Index(int? divisionId, int? groupId)
        {
            var teams = await _teamService.GetTeamsAsync();

            if (divisionId.HasValue)
            {
                teams = teams.Where(t => t.DivisionId == divisionId.Value);
            }

            if (groupId.HasValue)
            {
                teams = teams.Where(t => t.GroupId == groupId.Value);
            }

            return View(teams.ToList());
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var team = await _teamService.GetTeamAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            // Fetch matches for this team to display results
            var matches = await _matchService.GetMatchesForTeamAsync(id);
            ViewBag.Matches = matches;

            return View(team);
        }
    }
}
