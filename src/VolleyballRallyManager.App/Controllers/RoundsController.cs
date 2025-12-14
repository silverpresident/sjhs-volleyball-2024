using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Controllers;

/// <summary>
/// Public controller for displaying tournament rounds
/// </summary>
public class RoundsController : Controller
{
    private readonly IActiveTournamentService _activeTournamentService;
    private readonly ITournamentService _tournamentService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RoundsController> _logger;

    public RoundsController(
        IActiveTournamentService activeTournamentService,
        ITournamentService tournamentService,
        ApplicationDbContext context,
        ILogger<RoundsController> logger)
    {
        _activeTournamentService = activeTournamentService;
        _tournamentService = tournamentService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Displays all rounds for the active tournament, grouped by division
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            // Get active tournament
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                ViewBag.ErrorMessage = "No active tournament found.";
                return View(new PublicRoundsIndexViewModel());
            }

            // Get all divisions for the tournament
            var divisions = await _activeTournamentService.GetTournamentDivisionsAsync();

            var viewModel = new PublicRoundsIndexViewModel
            {
                TournamentId = activeTournament.Id,
                TournamentName = activeTournament.Name,
                DivisionGroups = new List<DivisionRoundsGroup>()
            };

            // For each division, get its rounds
            foreach (var division in divisions.OrderBy(d => d.Division.Name))
            {
                var divisionDetails = await _tournamentService.GetTournamentDivisionDetailsAsync(
                    activeTournament.Id,
                    division.Id);

                if (divisionDetails?.Rounds == null || !divisionDetails.Rounds.Any())
                {
                    continue; // Skip divisions with no rounds
                }

                var divisionGroup = new DivisionRoundsGroup
                {
                    DivisionId = division.Id,
                    DivisionName = division.Division.Name,
                    Rounds = new List<RoundSummaryInfo>()
                };

                // Build round summary for each round in the division
                foreach (var roundSummary in divisionDetails.Rounds.OrderBy(r => r.RoundNumber))
                {
                    // Get the full tournament round to access IsPlayoff and RoundId
                    var tournamentRound = await _context.TournamentRounds
                        .FirstOrDefaultAsync(tr => tr.Id == roundSummary.TournamentRoundId);

                    if (tournamentRound == null) continue;

                    // Get match statistics using RoundId
                    var matches = await _context.Matches
                        .Where(m => m.TournamentId == activeTournament.Id 
                                 && m.DivisionId == division.DivisionId
                                 && m.RoundId == tournamentRound.RoundId)
                        .ToListAsync();

                    var teamCount = await _context.TournamentRoundTeams
                        .Where(trt => trt.TournamentRoundId == roundSummary.TournamentRoundId)
                        .CountAsync();

                    var completedMatches = matches.Count(m => m.IsFinished);
                    var totalMatches = matches.Count;
                    var completionPercentage = totalMatches > 0 ? (double)completedMatches / totalMatches * 100 : 0;

                    divisionGroup.Rounds.Add(new RoundSummaryInfo
                    {
                        TournamentRoundId = roundSummary.TournamentRoundId,
                        RoundName = roundSummary.RoundName,
                        RoundNumber = roundSummary.RoundNumber,
                        IsPlayoff = tournamentRound.IsPlayoff,
                        MatchGenerationStrategy = roundSummary.MatchGenerationStrategy,
                        TeamCount = teamCount,
                        AdvancingTeamsCount = roundSummary.AdvancingTeamsCount,
                        TotalMatches = totalMatches,
                        CompletedMatches = completedMatches,
                        PendingMatches = totalMatches - completedMatches,
                        CompletionPercentage = completionPercentage,
                        IsFinished = roundSummary.IsFinished,
                        IsLocked = roundSummary.IsLocked
                    });
                }

                viewModel.DivisionGroups.Add(divisionGroup);
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading rounds index");
            ViewBag.ErrorMessage = "An error occurred while loading the rounds.";
            return View(new PublicRoundsIndexViewModel());
        }
    }

    /// <summary>
    /// Displays detailed information about a specific round
    /// </summary>
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var roundDetails = await _tournamentService.GetTournamentRoundDetailsAsync(id);
            if (roundDetails == null)
            {
                _logger.LogWarning("Round {RoundId} not found", id);
                return NotFound();
            }

            return View(roundDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading round details for {RoundId}", id);
            return View("Error");
        }
    }
}
