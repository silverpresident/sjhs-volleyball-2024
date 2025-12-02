using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.App.Areas.Admin.Models;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class TournamentRoundsController : Controller
{
    private readonly ITournamentRoundService _tournamentRoundService;
    private readonly IActiveTournamentService _activeTournamentService;
    private readonly IRanksService _ranksService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TournamentRoundsController> _logger;

    public TournamentRoundsController(
        ITournamentRoundService tournamentRoundService,
        IActiveTournamentService activeTournamentService,
        IRanksService ranksService,
        ApplicationDbContext context,
        ILogger<TournamentRoundsController> logger)
    {
        _tournamentRoundService = tournamentRoundService;
        _activeTournamentService = activeTournamentService;
        _ranksService = ranksService;
        _context = context;
        _logger = logger;
    }

    // GET: Admin/TournamentRounds/Index?tournamentId=...&divisionId=...
    public async Task<IActionResult> Index(Guid? tournamentId, Guid? divisionId)
    {
        try
        {
            // Get active tournament if not specified
            if (!tournamentId.HasValue)
            {
                var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
                tournamentId = activeTournament.Id;
            }

            // Get first division if not specified
            if (!divisionId.HasValue)
            {
                var firstDivision = await _context.Divisions.FirstOrDefaultAsync();
                if (firstDivision == null)
                {
                    TempData["ErrorMessage"] = "No divisions found. Please create a division first.";
                    return RedirectToAction("Index", "Home");
                }
                divisionId = firstDivision.Id;
            }

            var tournament = await _context.Tournaments.FindAsync(tournamentId.Value);
            var division = await _context.Divisions.FindAsync(divisionId.Value);

            var rounds = await _tournamentRoundService.GetTournamentRoundsAsync(tournamentId.Value, divisionId.Value);

            var roundViewModels = new List<TournamentRoundViewModel>();

            foreach (var round in rounds)
            {
                var teams = await _tournamentRoundService.GetRoundTeamsAsync(round.Id);
                var matches = await _tournamentRoundService.GetRoundMatchesAsync(round.Id);
                var completedMatches = matches.Count(m => m.IsFinished);

                var hasTeams = await _tournamentRoundService.HasTeamsAssignedAsync(round.Id);
                var hasMatches = await _tournamentRoundService.HasMatchesGeneratedAsync(round.Id);
                var allMatchesComplete = await _tournamentRoundService.AreAllMatchesCompleteAsync(round.Id);

                // Determine button visibility based on round state
                bool canFinalize = !round.IsFinished && hasMatches && allMatchesComplete;
                bool canSelectTeams = !hasTeams && round.PreviousTournamentRoundId.HasValue;
                bool canGenerateMatches = hasTeams && !hasMatches;
                bool canGenerateNextRound = round.IsFinished;

                // Check if previous round is finished for team selection
                if (canSelectTeams && round.PreviousTournamentRoundId.HasValue)
                {
                    var previousRound = await _tournamentRoundService.GetTournamentRoundByIdAsync(round.PreviousTournamentRoundId.Value);
                    canSelectTeams = previousRound != null && previousRound.IsFinished;
                }

                roundViewModels.Add(new TournamentRoundViewModel
                {
                    Id = round.Id,
                    TournamentName = tournament?.Name ?? "Unknown",
                    DivisionName = division?.Name ?? "Unknown",
                    RoundName = round.Round?.Name ?? $"Round {round.RoundNumber}",
                    RoundNumber = round.RoundNumber,
                    TeamCount = teams.Count,
                    MatchCount = matches.Count,
                    CompletedMatchCount = completedMatches,
                    IsFinished = round.IsFinished,
                    IsLocked = round.IsLocked,
                    TeamSelectionMethod = round.TeamSelectionMethod,
                    MatchGenerationStrategy = round.MatchGenerationStrategy,
                    TeamsAdvancing = round.TeamsAdvancing,
                    CanFinalize = canFinalize,
                    CanSelectTeams = canSelectTeams,
                    CanGenerateMatches = canGenerateMatches,
                    CanGenerateNextRound = canGenerateNextRound
                });
            }

            var viewModel = new TournamentRoundsIndexViewModel
            {
                TournamentId = tournamentId.Value,
                DivisionId = divisionId.Value,
                TournamentName = tournament?.Name ?? "Unknown",
                DivisionName = division?.Name ?? "Unknown",
                Rounds = roundViewModels
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tournament rounds index");
            TempData["ErrorMessage"] = "Error loading tournament rounds.";
            return RedirectToAction("Index", "Home");
        }
    }

    // GET: Admin/TournamentRounds/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var round = await _tournamentRoundService.GetTournamentRoundByIdAsync(id);
            if (round == null)
            {
                return NotFound();
            }

            var teams = await _tournamentRoundService.GetRoundTeamsAsync(id);
            var matches = await _tournamentRoundService.GetRoundMatchesAsync(id);

            var teamViewModels = teams.Select(t => new TournamentRoundTeamDetailsViewModel
            {
                TeamId = t.TeamId,
                TeamName = t.Team?.Name ?? "Unknown",
                SeedNumber = t.SeedNumber,
                FinalRank = t.FinalRank,
                Points = t.Points,
                MatchesPlayed = t.MatchesPlayed,
                Wins = t.Wins,
                Draws = t.Draws,
                Losses = t.Losses,
                SetsFor = t.SetsFor,
                SetsAgainst = t.SetsAgainst,
                SetsDifference = t.SetsDifference,
                ScoreFor = t.ScoreFor,
                ScoreAgainst = t.ScoreAgainst,
                ScoreDifference = t.ScoreDifference,
                GroupName = t.GroupName
            }).ToList();

            var hasTeams = await _tournamentRoundService.HasTeamsAssignedAsync(id);
            var hasMatches = await _tournamentRoundService.HasMatchesGeneratedAsync(id);
            var allMatchesComplete = await _tournamentRoundService.AreAllMatchesCompleteAsync(id);

            var viewModel = new TournamentRoundDetailsViewModel
            {
                Round = round,
                Teams = teamViewModels,
                Matches = matches,
                CanFinalize = !round.IsFinished && hasMatches && allMatchesComplete,
                CanSelectTeams = !hasTeams && round.PreviousTournamentRoundId.HasValue,
                CanGenerateMatches = hasTeams && !hasMatches,
                CanGenerateNextRound = round.IsFinished
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tournament round details for {RoundId}", id);
            TempData["ErrorMessage"] = "Error loading round details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Admin/TournamentRounds/CreateFirstRound?tournamentId=...&divisionId=...
    public async Task<IActionResult> CreateFirstRound(Guid? tournamentId, Guid? divisionId)
    {
        try
        {
            if (!tournamentId.HasValue)
            {
                var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
                tournamentId = activeTournament.Id;
            }

            if (!divisionId.HasValue)
            {
                var firstDivision = await _context.Divisions.FirstOrDefaultAsync();
                divisionId = firstDivision?.Id ?? Guid.Empty;
            }

            var rounds = await _context.Rounds.OrderBy(r => r.Sequence).ToListAsync();
            ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");

            var model = new CreateFirstRoundViewModel
            {
                TournamentId = tournamentId.Value,
                DivisionId = divisionId.Value
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create first round form");
            TempData["ErrorMessage"] = "Error loading form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Admin/TournamentRounds/CreateFirstRound
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFirstRound(CreateFirstRoundViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var rounds = await _context.Rounds.OrderBy(r => r.Sequence).ToListAsync();
                ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");
                return View(model);
            }

            var userName = User.Identity?.Name ?? "admin";

            var tournamentRound = await _tournamentRoundService.CreateFirstRoundAsync(
                model.TournamentId,
                model.DivisionId,
                model.RoundId,
                model.TeamSelectionMethod,
                model.MatchGenerationStrategy,
                model.TeamsAdvancing,
                userName);

            TempData["SuccessMessage"] = $"First round created successfully with {tournamentRound.TournamentRoundTeams.Count} teams.";
            
            // Redirect to generate matches
            return RedirectToAction(nameof(GenerateMatches), new { id = tournamentRound.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating first round");
            ModelState.AddModelError("", $"Error creating first round: {ex.Message}");
            
            var rounds = await _context.Rounds.OrderBy(r => r.Sequence).ToListAsync();
            ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");
            return View(model);
        }
    }

    // GET: Admin/TournamentRounds/GenerateNextRound/5
    public async Task<IActionResult> GenerateNextRound(Guid id)
    {
        try
        {
            var currentRound = await _tournamentRoundService.GetTournamentRoundByIdAsync(id);
            if (currentRound == null)
            {
                return NotFound();
            }

            if (!currentRound.IsFinished)
            {
                TempData["ErrorMessage"] = "Current round must be finalized before generating next round.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var rounds = await _context.Rounds.OrderBy(r => r.Sequence).ToListAsync();
            ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");

            var model = new CreateNextRoundViewModel
            {
                TournamentId = currentRound.TournamentId,
                DivisionId = currentRound.DivisionId,
                PreviousTournamentRoundId = id,
                PreviousRoundName = currentRound.Round?.Name ?? $"Round {currentRound.RoundNumber}",
                TeamsAdvancing = currentRound.TeamsAdvancing / 2 // Default to half
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading generate next round form");
            TempData["ErrorMessage"] = "Error loading form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Admin/TournamentRounds/GenerateNextRound
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateNextRound(CreateNextRoundViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var rounds = await _context.Rounds.OrderBy(r => r.Sequence).ToListAsync();
                ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");
                return View(model);
            }

            var userName = User.Identity?.Name ?? "admin";

            var tournamentRound = await _tournamentRoundService.CreateNextRoundAsync(
                model.TournamentId,
                model.DivisionId,
                model.RoundId,
                model.PreviousTournamentRoundId,
                model.TeamSelectionMethod,
                model.MatchGenerationStrategy,
                model.TeamsAdvancing,
                userName);

            TempData["SuccessMessage"] = "Next round created successfully.";
            
            // Redirect to select teams
            return RedirectToAction(nameof(SelectTeams), new { id = tournamentRound.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next round");
            ModelState.AddModelError("", $"Error generating next round: {ex.Message}");
            
            var rounds = await _context.Rounds.OrderBy(r => r.Sequence).ToListAsync();
            ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");
            return View(model);
        }
    }

    // POST: Admin/TournamentRounds/SelectTeams/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectTeams(Guid id)
    {
        try
        {
            var userName = User.Identity?.Name ?? "admin";
            var teams = await _tournamentRoundService.SelectTeamsForRoundAsync(id, userName);

            TempData["SuccessMessage"] = $"Selected {teams.Count} teams for the round.";
            
            // Redirect to generate matches
            return RedirectToAction(nameof(GenerateMatches), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting teams for round {RoundId}", id);
            TempData["ErrorMessage"] = $"Error selecting teams: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // GET: Admin/TournamentRounds/GenerateMatches/5
    public async Task<IActionResult> GenerateMatches(Guid id)
    {
        try
        {
            var round = await _tournamentRoundService.GetTournamentRoundByIdAsync(id);
            if (round == null)
            {
                return NotFound();
            }

            var teams = await _tournamentRoundService.GetRoundTeamsAsync(id);

            var model = new GenerateMatchesViewModel
            {
                TournamentRoundId = id,
                RoundName = round.Round?.Name ?? $"Round {round.RoundNumber}",
                StartTime = DateTime.Now.AddMinutes(30),
                CourtLocation = "Court 1",
                TeamCount = teams.Count,
                Strategy = round.MatchGenerationStrategy
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading generate matches form");
            TempData["ErrorMessage"] = "Error loading form.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // POST: Admin/TournamentRounds/GenerateMatches
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateMatches(GenerateMatchesViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userName = User.Identity?.Name ?? "admin";
            var matches = await _tournamentRoundService.GenerateMatchesForRoundAsync(
                model.TournamentRoundId,
                model.StartTime,
                model.CourtLocation,
                userName);

            TempData["SuccessMessage"] = $"Generated {matches.Count} matches for the round.";
            
            // Redirect to round details
            return RedirectToAction(nameof(Details), new { id = model.TournamentRoundId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating matches");
            ModelState.AddModelError("", $"Error generating matches: {ex.Message}");
            return View(model);
        }
    }

    // POST: Admin/TournamentRounds/FinalizeRound/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizeRound(Guid id)
    {
        try
        {
            var userName = User.Identity?.Name ?? "admin";
            var round = await _tournamentRoundService.FinalizeRoundAsync(id, userName);

            TempData["SuccessMessage"] = "Round finalized successfully. Team rankings have been calculated.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing round {RoundId}", id);
            TempData["ErrorMessage"] = $"Error finalizing round: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // POST: Admin/TournamentRounds/RankTeams/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RankTeams(Guid id)
    {
        try
        {
            var round = await _tournamentRoundService.GetTournamentRoundByIdAsync(id);
            if (round == null)
            {
                return NotFound();
            }

            if (round.IsFinished)
            {
                TempData["ErrorMessage"] = "Cannot rank teams for a finalized round.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var rankedTeams = await _ranksService.UpdateTeamRanksAsync(id);

            TempData["SuccessMessage"] = $"Team rankings updated successfully. {rankedTeams.Count} teams ranked.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ranking teams for round {RoundId}", id);
            TempData["ErrorMessage"] = $"Error ranking teams: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
