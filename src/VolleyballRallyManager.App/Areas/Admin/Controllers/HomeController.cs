using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Common;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IMatchService _matchService;
        private readonly IBulletinService _bulletinService;
        private readonly ITeamService _teamService;
        private readonly ApplicationDbContext _context;
        private readonly IActiveTournamentService _activeTournamentService;

        public HomeController(
            IMatchService matchService,
            IBulletinService bulletinService,
            ITeamService teamService,
            ApplicationDbContext context,
            IActiveTournamentService activeTournamentService)
        {
            _matchService = matchService;
            _bulletinService = bulletinService;
            _teamService = teamService;
            _context = context;
            _activeTournamentService = activeTournamentService;
        }

        public async Task<IActionResult> Index()
        {
         
            // Get teams count
            var teams = await _teamService.GetTeamsAsync();
            var totalTeams = teams.Count();

            // Get teams by division
            var teamsByDivision = await _context.TournamentTeamDivisions
                .Include(ttd => ttd.Division)
                .GroupBy(ttd => ttd.Division.Name)
                .Select(g => new { Division = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Division, x => x.Count);

            var viewModel = new DashboardViewModel
            {
                TotalTeams = totalTeams,

                TotalMatches = await _activeTournamentService.MatchCountAsync(),
                MatchesInProgress = await _activeTournamentService.MatchCountAsync(MatchState.InProgress),
                DisputedMatches = await _activeTournamentService.MatchCountAsync(MatchState.Disputed),
                MatchesFinished = await _activeTournamentService.MatchCountAsync(MatchState.Finished),
                RecentMatches = await _activeTournamentService.RecentMatchesAsync(),
                RecentBulletins = await _bulletinService.GetRecentAsync(5),
                TeamsByDivision = teamsByDivision
            };

            return View(viewModel);
        }
    }
}
