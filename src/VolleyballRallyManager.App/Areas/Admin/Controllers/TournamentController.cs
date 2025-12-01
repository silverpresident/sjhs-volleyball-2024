using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TournamentController : Controller
    {
        private readonly ITournamentService _tournamentService;
        private readonly ApplicationDbContext _context;

        public TournamentController(ITournamentService tournamentService, ApplicationDbContext context)
        {
            _tournamentService = tournamentService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tournaments = await _tournamentService.GetAllTournamentsAsync();
            return View(tournaments);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                await _tournamentService.CreateTournamentAsync(tournament);
                return RedirectToAction(nameof(Index));
            }
            return View(tournament);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var tournament = await _tournamentService.GetTournamentByIdAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }

            // Populate division statistics
            var divisionStats = await _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == id)
                .Include(ttd => ttd.Division)
                .GroupBy(ttd => new { ttd.DivisionId, DivisionName = ttd.Division.Name })
                .Select(g => new TournamentDivisionViewModel
                {
                    DivisionId = g.Key.DivisionId,
                    DivisionName = g.Key.DivisionName,
                    GroupNames = g.Select(x => x.GroupName).Distinct().OrderBy(gn => gn).ToList(),
                    TeamCount = g.Count(),
                    MatchesPlayed = _context.Matches
                        .Count(m => m.TournamentId == id 
                                 && m.DivisionId == g.Key.DivisionId 
                                 && m.IsFinished)
                })
                .OrderBy(d => d.DivisionName)
                .ToListAsync();

            // Populate round statistics grouped by division
            var roundStats = await _context.Matches
                .Where(m => m.TournamentId == id)
                .Include(m => m.Round)
                .GroupBy(m => new { m.RoundId, m.DivisionId, m.Round!.Name, m.Round.Sequence })
                .Select(g => new
                {
                    RoundId = g.Key.RoundId,
                    DivisionId = g.Key.DivisionId,
                    RoundName = g.Key.Name,
                    Sequence = g.Key.Sequence,
                    TeamIds = g.Select(m => m.HomeTeamId).Union(g.Select(m => m.AwayTeamId)).Distinct().ToList(),
                    MatchesScheduled = g.Count(),
                    MatchesPlayed = g.Count(m => m.IsFinished)
                })
                .ToListAsync();

            // Get division names for rounds
            var divisionNames = await _context.Divisions
                .Where(d => roundStats.Select(r => r.DivisionId).Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.Name);

            var rounds = roundStats.Select(r => new TournamentRoundViewModel
            {
                RoundId = r.RoundId,
                DivisionId = r.DivisionId,
                DivisionName = divisionNames.ContainsKey(r.DivisionId) ? divisionNames[r.DivisionId] : "Unknown",
                RoundName = r.RoundName,
                Sequence = r.Sequence,
                TeamCount = r.TeamIds.Count,
                MatchesScheduled = r.MatchesScheduled,
                MatchesPlayed = r.MatchesPlayed
            })
            .OrderBy(r => r.DivisionName)
            .ThenBy(r => r.Sequence)
            .ToList();

            // Get all teams in tournament
            var teams = await _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == id)
                .Include(ttd => ttd.Team)
                .Include(ttd => ttd.Division)
                .OrderBy(ttd => ttd.Division.Name)
                .ThenBy(ttd => ttd.GroupName)
                .ThenBy(ttd => ttd.Team.Name)
                .ToListAsync();

            var viewModel = new TournamentDetailsViewModel
            {
                TournamentId = id,
                Tournament = tournament,
                Divisions = divisionStats,
                Rounds = rounds,
                Teams = teams
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var tournament = await _tournamentService.GetTournamentByIdAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }
            return View(tournament);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                await _tournamentService.UpdateTournamentAsync(tournament);
                return RedirectToAction(nameof(Index));
            }
            return View(tournament);
        }

        public async Task<IActionResult> SetActive(Guid id)
        {
            await _tournamentService.SetActiveTournamentAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
