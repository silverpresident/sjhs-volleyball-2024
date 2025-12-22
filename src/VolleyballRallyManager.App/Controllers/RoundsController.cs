using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;
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
                    division.DivisionId);

                if (divisionDetails?.Rounds == null || !divisionDetails.Rounds.Any())
                {
                    continue; // Skip divisions with no rounds
                }

                var divisionGroup = new DivisionRoundsGroup
                {
                    DivisionId = division.DivisionId,
                    DivisionName = division.Division.Name,
                    Rounds = new List<TournamentRoundSummaryViewModel>()
                };

                // Build round summary for each round in the division
                foreach (var roundSummary in divisionDetails.Rounds.OrderBy(r => r.RoundNumber))
                {
                    divisionGroup.Rounds.Add(roundSummary);
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
            if (roundDetails.CurrentRound.RoundNumber == 1)
            {
                roundDetails.Teams = roundDetails.Teams.OrderBy(t => t.GroupName).ToList();
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
