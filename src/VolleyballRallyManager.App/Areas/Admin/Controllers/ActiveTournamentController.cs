using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActiveTournamentController : Controller
    {
        private readonly IActiveTournamentService _activeTournamentService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ActiveTournamentController> _logger;

        public ActiveTournamentController(IActiveTournamentService activeTournamentService, ApplicationDbContext context, ILogger<ActiveTournamentController> logger)
        {
            _activeTournamentService = activeTournamentService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Executing Index action");
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                _logger.LogWarning("Active tournament not found");
                return NotFound();
            }

            var tournamentId = activeTournament.Id;

            // Populate division statistics
            var divisionStats = await _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == tournamentId)
                .Include(ttd => ttd.Division)
                .GroupBy(ttd => new { ttd.DivisionId, DivisionName = ttd.Division.Name })
                .Select(g => new TournamentDivisionViewModel
                {
                    DivisionId = g.Key.DivisionId,
                    DivisionName = g.Key.DivisionName,
                    GroupNames = g.Select(x => x.GroupName).Distinct().OrderBy(gn => gn).ToList(),
                    TeamCount = g.Count(),
                    RoundsCount = _context.TournamentRounds
                        .Count(m => m.TournamentId == tournamentId 
                                 && m.DivisionId == g.Key.DivisionId),
                    MatchCount = _context.Matches
                        .Count(m => m.TournamentId == tournamentId 
                                 && m.DivisionId == g.Key.DivisionId),
                    MatchesPlayed = _context.Matches
                        .Count(m => m.TournamentId == tournamentId 
                                 && m.DivisionId == g.Key.DivisionId 
                                 && m.IsFinished)
                })
                .OrderBy(d => d.DivisionName)
                .ToListAsync();

            // Populate round statistics grouped by division
            var roundStats = await _context.Matches
                .Where(m => m.TournamentId == tournamentId)
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
                .Where(ttd => ttd.TournamentId == tournamentId)
                .Include(ttd => ttd.Team)
                .Include(ttd => ttd.Division)
                .OrderBy(ttd => ttd.Division.Name)
                .ThenBy(ttd => ttd.GroupName)
                .ThenBy(ttd => ttd.Team.Name)
                .ToListAsync();

            var viewModel = new TournamentDetailsViewModel
            {
                TournamentId = tournamentId,
                Tournament = activeTournament,
                Divisions = divisionStats,
                Rounds = rounds,
                Teams = teams
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SelectDivisions()
        {
            _logger.LogInformation("Executing SelectDivisions action");
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                _logger.LogWarning("Active tournament not found");
                return NotFound();
            }

            var availableDivisions = await _activeTournamentService.GetAvailableDivisionsAsync();
            var viewModel = new SelectDivisionsViewModel
            {
                Tournament = activeTournament,
                AvailableDivisions = availableDivisions,
                SelectedDivisionIds = activeTournament.TournamentDivisions.Select(td => td.DivisionId).ToList() ?? new List<Guid>()
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SelectDivisions([Bind("SelectedDivisionIds")] SelectDivisionsViewModel model)
        {
            _logger.LogInformation("Executing SelectDivisions post!!");
            if (ModelState.IsValid)
            {
                var divisionIds = model.SelectedDivisionIds;
                await _activeTournamentService.UpdateTournamentDivisionsAsync(divisionIds);
                return RedirectToAction(nameof(Index));
            }
            model.AvailableDivisions = await _activeTournamentService.GetAvailableDivisionsAsync();
            return View(model);
        }
    }
}
