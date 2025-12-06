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
    private readonly ITournamentService _tournamentService;
    private readonly IRanksService _ranksService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TournamentRoundsController> _logger;

    public TournamentRoundsController(
        ITournamentRoundService tournamentRoundService,
        IActiveTournamentService activeTournamentService,
        IRanksService ranksService,
        ApplicationDbContext context,
        ILogger<TournamentRoundsController> logger,
        ITournamentService tournamentService)
    {
        _tournamentRoundService = tournamentRoundService;
        _activeTournamentService = activeTournamentService;
        _ranksService = ranksService;
        _context = context;
        _logger = logger;
        _tournamentService = tournamentService;
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

            var viewModel = new TournamentRoundsIndexViewModel
            {
                TournamentId = tournamentId.Value,
                DivisionId = divisionId.Value,
                TournamentName = tournament?.Name ?? "Unknown",
                DivisionName = division?.Name ?? "Unknown",
            };
            
            var tournamentDivisionDetails = await _tournamentService.GetTournamentDivisionDetailsAsync(tournamentId.Value, divisionId.Value);
            if (tournamentDivisionDetails != null){
                viewModel.DivisionName = tournamentDivisionDetails.DivisionName;
                viewModel.Rounds = tournamentDivisionDetails.Rounds;
            }
            ViewBag.Divisions = await _activeTournamentService.GetTournamentDivisionsAsync();
           
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

            var viewModel = await _tournamentService.GetTournamentRoundDetailsAsync(id);
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

            // ACCESS GUARD: Check if a round with RoundNumber > 1 already exists
            var existingRounds = await _context.TournamentRounds
                .Include(tr => tr.Round)
                .Where(tr => tr.TournamentId == tournamentId.Value && tr.DivisionId == divisionId.Value)
                .ToListAsync();

            if (existingRounds.Any(tr => tr.Round != null && tr.Round.Sequence > 1))
            {
                TempData["ErrorMessage"] = "Cannot create first round. Subsequent rounds already exist for this division. Please edit Round 1 instead.";
                
                // Find Round 1 to redirect to edit
                var firstRound = existingRounds.FirstOrDefault(tr => tr.Round != null && tr.Round.Sequence == 1);
                if (firstRound != null)
                {
                    return RedirectToAction(nameof(Edit), new { id = firstRound.Id });
                }
                
                return RedirectToAction(nameof(Index), new { tournamentId, divisionId });
            }

            // Check if Round 1 already exists
            var round1 = existingRounds.FirstOrDefault(tr => tr.Round != null && tr.Round.Sequence == 1);
            if (round1 != null)
            {
                TempData["ErrorMessage"] = "First round already exists. Redirecting to edit.";
                return RedirectToAction(nameof(Edit), new { id = round1.Id });
            }

            var tournament = await _context.Tournaments.FindAsync(tournamentId.Value);
            var division = await _context.Divisions.FindAsync(divisionId.Value);
            
            // Get Round with RoundNumber = 1
            var firstRoundDef = await _context.Rounds.FirstOrDefaultAsync(r => r.Sequence == 1);
            if (firstRoundDef == null)
            {
                TempData["ErrorMessage"] = "No round with Sequence = 1 found. Please create a round first.";
                return RedirectToAction(nameof(Index), new { tournamentId, divisionId });
            }

            // Count teams in division
            var teamsCount = await _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == tournamentId.Value && ttd.DivisionId == divisionId.Value)
                .CountAsync();

            var model = new CreateFirstRoundViewModel
            {
                TournamentId = tournamentId.Value,
                DivisionId = divisionId.Value,
                RoundId = firstRoundDef.Id,
                TournamentName = tournament?.Name ?? "Unknown",
                DivisionName = division?.Name ?? "Unknown",
                RoundName = firstRoundDef.Name,
                TotalTeamsInDivision = teamsCount,
                GroupConfigurationValue = Math.Max(2, teamsCount / 4) // Default to 4 groups
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
                return View(model);
            }

            var userName = User.Identity?.Name ?? "admin";

            // Create the tournament round
            var tournamentRound = await _tournamentRoundService.CreateFirstRoundAsync(
                model.TournamentId,
                model.DivisionId,
                model.RoundId,
                model.AdvancingTeamsCount,
                model.AdvancingTeamSelectionStrategy,
                model.MatchGenerationStrategy,
                userName);

            // Save group configuration
            tournamentRound.TeamsPerGroup = model.GroupConfigurationType == "TeamsPerGroup" ? model.GroupConfigurationValue : null;
            tournamentRound.GroupsInRound = model.GroupConfigurationType == "GroupsInRound" ? model.GroupConfigurationValue : null;
            _context.TournamentRounds.Update(tournamentRound);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"First round created successfully with {tournamentRound.TournamentRoundTeams.Count} teams.";

            // Immediate execution: Assign teams if requested
            if (model.AssignTeamsNow)
            {
                await _tournamentRoundService.AssignFirstRoundTeamsAsync(tournamentRound.Id, userName);
                _logger.LogInformation("Teams assigned immediately for round {RoundId}", tournamentRound.Id);
            }

            // Immediate execution: Generate matches if requested
            if (model.GenerateMatchesNow)
            {
                return RedirectToAction(nameof(GenerateMatches), new { id = tournamentRound.Id });
            }
            
            return RedirectToAction(nameof(Details), new { id = tournamentRound.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating first round");
            ModelState.AddModelError("", $"Error creating first round: {ex.Message}");
            return View(model);
        }
    }

    // GET: Admin/TournamentRounds/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var round = await _tournamentRoundService.GetTournamentRoundByIdAsync(id);
            if (round == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments.FindAsync(round.TournamentId);
            var division = await _context.Divisions.FindAsync(round.DivisionId);
            
            // Count teams in division
            var teamsCount = await _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == round.TournamentId && ttd.DivisionId == round.DivisionId)
                .CountAsync();

            var hasTeams = await _tournamentRoundService.HasTeamsAssignedAsync(id);
            var hasMatches = await _tournamentRoundService.HasMatchesGeneratedAsync(id);

            // Determine group configuration type and value
            string groupConfigType = "TeamsPerGroup";
            int groupConfigValue = 2;
            
            if (round.GroupsInRound.HasValue && round.GroupsInRound.Value > 0)
            {
                groupConfigType = "GroupsInRound";
                groupConfigValue = round.GroupsInRound.Value;
            }
            else if (round.TeamsPerGroup.HasValue && round.TeamsPerGroup.Value > 0)
            {
                groupConfigType = "TeamsPerGroup";
                groupConfigValue = round.TeamsPerGroup.Value;
            }

            var model = new EditTournamentRoundViewModel
            {
                Id = round.Id,
                TournamentId = round.TournamentId,
                DivisionId = round.DivisionId,
                RoundId = round.RoundId,
                TournamentName = tournament?.Name ?? "Unknown",
                DivisionName = division?.Name ?? "Unknown",
                RoundName = round.Round?.Name ?? $"Round {round.RoundNumber}",
                TotalTeamsInDivision = teamsCount,
                RoundNumber = round.RoundNumber,
                AdvancingTeamSelectionStrategy = round.AdvancingTeamSelectionStrategy,
                MatchGenerationStrategy = round.MatchGenerationStrategy,
                AdvancingTeamsCount = round.AdvancingTeamsCount,
                GroupConfigurationType = groupConfigType,
                GroupConfigurationValue = groupConfigValue,
                IsFinished = round.IsFinished,
                HasTeams = hasTeams,
                HasMatches = hasMatches
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit round form for {RoundId}", id);
            TempData["ErrorMessage"] = "Error loading edit form.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // POST: Admin/TournamentRounds/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTournamentRoundViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tournamentRound = await _tournamentRoundService.GetTournamentRoundByIdAsync(model.Id);
            if (tournamentRound == null)
            {
                return NotFound();
            }

            if (tournamentRound.IsFinished)
            {
                TempData["ErrorMessage"] = "Cannot edit a finalized round.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            var userName = User.Identity?.Name ?? "admin";

            // Update round properties
            tournamentRound.AdvancingTeamSelectionStrategy = model.AdvancingTeamSelectionStrategy;
            tournamentRound.MatchGenerationStrategy = model.MatchGenerationStrategy;
            tournamentRound.AdvancingTeamsCount = model.AdvancingTeamsCount;
            
            // Update group configuration
            tournamentRound.TeamsPerGroup = model.GroupConfigurationType == "TeamsPerGroup" ? model.GroupConfigurationValue : null;
            tournamentRound.GroupsInRound = model.GroupConfigurationType == "GroupsInRound" ? model.GroupConfigurationValue : null;
            
            tournamentRound.UpdatedAt = DateTime.UtcNow;
            tournamentRound.UpdatedBy = userName;

            _context.TournamentRounds.Update(tournamentRound);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Round updated successfully.";

            // Immediate execution: Assign teams if requested
            if (model.AssignTeamsNow && !model.HasTeams)
            {
                try
                {
                    if (tournamentRound.RoundNumber == 1)
                    {
                        await _tournamentRoundService.AssignFirstRoundTeamsAsync(tournamentRound.Id, userName);
                        TempData["SuccessMessage"] += $"First round teams assigned.";
                    }
                    else
                    {
                        var teams = await _tournamentRoundService.SelectTeamsForRoundAsync(model.Id, userName);
                        TempData["SuccessMessage"] += $" {teams.Count} teams assigned.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error assigning teams during edit");
                    TempData["ErrorMessage"] = $"Round updated but error assigning teams: {ex.Message}";
                }
            }

            // Immediate execution: Generate matches if requested
            if (model.GenerateMatchesNow && !model.HasMatches)
            {
                return RedirectToAction(nameof(GenerateMatches), new { id = model.Id });
            }

            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing round {RoundId}", model.Id);
            ModelState.AddModelError("", $"Error editing round: {ex.Message}");
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

            var tournament = await _context.Tournaments.FindAsync(currentRound.TournamentId);
            var division = await _context.Divisions.FindAsync(currentRound.DivisionId);

            // Get all rounds and the current round to disable it in the dropdown
            var allRounds = await _context.Rounds.OrderBy(r => r.Sequence).ToListAsync();
            ViewData["Rounds"] = new SelectList(allRounds, "Id", "Name");

            // Get team count from current round
            var currentTeamCount = await _tournamentRoundService.GetRoundTeamsAsync(id);

            var model = new CreateNextRoundViewModel
            {
                TournamentId = currentRound.TournamentId,
                DivisionId = currentRound.DivisionId,
                PreviousTournamentRoundId = id,
                CurrentRoundId = currentRound.RoundId,
                TournamentName = tournament?.Name ?? "Unknown",
                DivisionName = division?.Name ?? "Unknown",
                PreviousRoundName = currentRound.Round?.Name ?? $"Round {currentRound.RoundNumber}",
                
                // SOURCE: Teams coming into the NEW round (based on current round's advancing settings)
                SourceTeamCount = currentRound.AdvancingTeamsCount,
                SourceSelectionMethod = currentRound.AdvancingTeamSelectionStrategy,
                SourceMatchStrategy = currentRound.MatchGenerationStrategy,
                
                // DESTINATION: Default settings for teams advancing from the NEW round
                AdvancingTeamsCount = Math.Max(2, currentRound.AdvancingTeamsCount / 2), // Default to half, minimum 2
                AdvancingTeamSelectionStrategy = currentRound.AdvancingTeamSelectionStrategy, // Same as current
                MatchGenerationStrategy = currentRound.MatchGenerationStrategy, // Same as current
                GroupConfigurationValue = Math.Max(2, currentRound.AdvancingTeamsCount / 4), // Default sensible groups
                
                // Default immediate actions
                AssignTeamsNow = true,
                GenerateMatchesNow = true
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

            // Create the next tournament round
            var tournamentRound = await _tournamentRoundService.CreateNextRoundAsync(
                model.TournamentId,
                model.DivisionId,
                model.RoundId,
                model.PreviousTournamentRoundId,
                model.AdvancingTeamSelectionStrategy,
                model.MatchGenerationStrategy,
                model.AdvancingTeamsCount,
                userName);

            // Save group configuration
            tournamentRound.TeamsPerGroup = model.GroupConfigurationType == "TeamsPerGroup" ? model.GroupConfigurationValue : null;
            tournamentRound.GroupsInRound = model.GroupConfigurationType == "GroupsInRound" ? model.GroupConfigurationValue : null;
            _context.TournamentRounds.Update(tournamentRound);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Next round created successfully.";
            
            // Immediate execution: Assign teams if requested (SelectTeams workflow)
            if (model.AssignTeamsNow)
            {
                try
                {
                    var teams = await _tournamentRoundService.SelectTeamsForRoundAsync(tournamentRound.Id, userName);
                    TempData["SuccessMessage"] += $" {teams.Count} teams selected and assigned.";
                    _logger.LogInformation("Teams selected immediately for round {RoundId}", tournamentRound.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error selecting teams during next round creation");
                    TempData["ErrorMessage"] = $"Round created but error selecting teams: {ex.Message}";
                    return RedirectToAction(nameof(Details), new { id = tournamentRound.Id });
                }
            }

            // Immediate execution: Generate matches if requested
            if (model.GenerateMatchesNow)
            {
                return RedirectToAction(nameof(GenerateMatches), new { id = tournamentRound.Id });
            }
            
            return RedirectToAction(nameof(Details), new { id = tournamentRound.Id });
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
            //TODO if round 1 auto assign teams
            var teams = await _tournamentRoundService.GetRoundTeamsAsync(id);

            var model = new GenerateMatchesViewModel
            {
                TournamentRoundId = id,
                RoundName = round.Round?.Name ?? $"Round {round.RoundNumber}",
                StartTime = DateTime.Now.AddMinutes(30),
                StartingCourtNumber = 1,
                NumberOfCourts = 3,
                MatchTimeInterval = 10,
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
                model.StartingCourtNumber,
                model.NumberOfCourts,
                model.MatchTimeInterval,
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
