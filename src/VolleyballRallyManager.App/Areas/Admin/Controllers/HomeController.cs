using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IMatchService _matchService;
        private readonly IAnnouncementService _announcementService;
        private readonly ITeamService _teamService;
        private readonly ApplicationDbContext _context;

        public HomeController(
            IMatchService matchService,
            IAnnouncementService announcementService,
            ITeamService teamService,
            ApplicationDbContext context)
        {
            _matchService = matchService;
            _announcementService = announcementService;
            _teamService = teamService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get all matches
            var allMatches = await _matchService.GetMatchesAsync();
            var inProgressMatches = await _matchService.GetInProgressMatchesAsync();
            var finishedMatches = await _matchService.GetFinishedMatchesAsync();
            var disputedMatches = await _matchService.GetDisputedMatchesAsync();

            // Get recent matches (last 5)
            var recentMatches = allMatches
                .OrderByDescending(m => m.ScheduledTime)
                .Take(5)
                .ToList();

            // Get recent announcements (last 5)
            var allAnnouncements = await _announcementService.GetAllAnnouncementsAsync(includeHidden: false);
            var recentAnnouncements = allAnnouncements
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToList();

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
                TotalMatches = allMatches.Count,
                MatchesInProgress = inProgressMatches.Count,
                MatchesFinished = finishedMatches.Count,
                DisputedMatches = disputedMatches.Count,
                RecentMatches = recentMatches,
                RecentAnnouncements = recentAnnouncements,
                TeamsByDivision = teamsByDivision
            };

            return View(viewModel);
        }
    }
}
