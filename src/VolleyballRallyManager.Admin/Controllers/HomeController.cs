using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.App.Models;

namespace VolleyballRallyManager.App.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var divisions = await _context.Divisions.ToListAsync();
        var teamsByDivision = new Dictionary<string, int>();

        foreach (var division in divisions)
        {
            var teamCount = await _context.Teams.CountAsync(t => t.Division.Id == division.Id);
            teamsByDivision[division.Name] = teamCount;
        }

        var dashboard = new DashboardViewModel
        {
            TotalTeams = await _context.Teams.CountAsync(),
            TotalMatches = await _context.Matches.CountAsync(),
            MatchesInProgress = await _context.Matches
                .CountAsync(m => m.ActualStartTime.HasValue && !m.IsFinished),
            MatchesFinished = await _context.Matches.CountAsync(m => m.IsFinished),
            DisputedMatches = await _context.Matches.CountAsync(m => m.IsDisputed),
            
            RecentMatches = await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Round)
                .OrderByDescending(m => m.UpdatedAt)
                .Take(5)
                .ToListAsync(),

            RecentAnnouncements = await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync(),

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
