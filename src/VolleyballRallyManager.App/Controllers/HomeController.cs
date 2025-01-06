using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Services;
using VolleyballRallyManager.App.Models;

namespace VolleyballRallyManager.App.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly IActiveTournamentService _activeTournamentService;

    public HomeController(ApplicationDbContext context, IActiveTournamentService activeTournamentService, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
        _activeTournamentService = activeTournamentService;
    }

    public async Task<IActionResult> Index()
    {
        var teamsByDivision = new Dictionary<string, int>();
        var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
        if (activeTournament != null)
        {
            var divisions = await _activeTournamentService.GetTournamentDivisionsAsync();

            foreach (var division in divisions)
            {
                teamsByDivision[division.Division.Name] = await _activeTournamentService.GetTournamentTeamsCountAsync(division.DivisionId);
            }
        }



        var dashboard = new DashboardViewModel
        {
            TotalTeams = await _activeTournamentService.TeamCountAsync(),
            TotalMatches = await _activeTournamentService.MatchCountAsync(),
            /*             MatchesInProgress = await _context.Matches
                            .CountAsync(m => m.ActualStartTime.HasValue && !m.IsFinished),
                        MatchesFinished = await _context.Matches.CountAsync(m => m.IsFinished),
                        DisputedMatches = await _context.Matches.CountAsync(m => m.IsDisputed),

                        RecentMatches = await _context.Matches
                            .Include(m => m.HomeTeam)
                            .Include(m => m.AwayTeam)
                            .Include(m => m.Round)
                            .OrderByDescending(m => m.ScheduledTime)
                            .Take(5)
                            .ToListAsync(),

                        RecentAnnouncements = await _context.Announcements
                            .OrderByDescending(a => a.CreatedAt)
                            .Take(5)
                            .ToListAsync(),*/

            TeamsByDivision = teamsByDivision
        };

        return View(dashboard);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
