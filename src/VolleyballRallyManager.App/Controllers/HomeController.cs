using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Common;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly IActiveTournamentService _activeTournamentService;
    private readonly IBulletinService _bulletinService;

    public HomeController(ApplicationDbContext context, IActiveTournamentService activeTournamentService, ILogger<HomeController> logger, IBulletinService bulletinService)
    {
        _context = context;
        _logger = logger;
        _activeTournamentService = activeTournamentService;
        _bulletinService = bulletinService;
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
            MatchesInProgress = await _activeTournamentService.MatchCountAsync(MatchState.InProgress),
            DisputedMatches = await _activeTournamentService.MatchCountAsync(MatchState.Disputed),
            MatchesFinished = await _activeTournamentService.MatchCountAsync(MatchState.Finished),
            RecentMatches = await _activeTournamentService.RecentMatchesAsync(),
            RecentBulletins = await _bulletinService.GetRecentAsync(5),
            TeamsByDivision = teamsByDivision
        };
        if (activeTournament != null)
        {
            dashboard.ActiveTournamentName = activeTournament.Name;
        }

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
