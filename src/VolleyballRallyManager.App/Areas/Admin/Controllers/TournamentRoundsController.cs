using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.App.Areas.Admin.Models;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;
using VolleyballRallyManager.Lib.Workers;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class TournamentRoundsController : Controller
{
    private readonly TournamentChannel _tournamentChannel;
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
        ITournamentService tournamentService,
        TournamentChannel tournamentChannel)
    {
        _tournamentRoundService = tournamentRoundService;
        _activeTournamentService = activeTournamentService;
        _ranksService = ranksService;
        _context = context;
        _logger = logger;
        _tournamentService = tournamentService;
        _tournamentChannel = tournamentChannel;
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
                var firstDivision = await _context.Divisions.AsNoTracking().FirstOrDefaultAsync();
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
                TournamentName = tournament?.Name ?? "Unknown Tournament",
                DivisionName = division?.Name ?? "Unknown Division",
            };
            
            var tournamentDivisionDetails = await _tournamentService.GetTournamentDivisionDetailsAsync(tournamentId.Value, divisionId.Value);
            if (tournamentDivisionDetails != null){
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
                var firstDivision = await _context.Divisions.AsNoTracking().FirstOrDefaultAsync();
                divisionId = firstDivision?.Id ?? Guid.Empty;
            }

            // ACCESS GUARD: Check if a tournamentRound with RoundNumber > 1 already exists
            var existingRounds = await _context.TournamentRounds
                .Include(tr => tr.Round)
                .AsNoTracking()
                .Where(tr => tr.TournamentId == tournamentId.Value && tr.DivisionId == divisionId.Value)
                .ToListAsync();

            if (existingRounds.Any(tr => tr.Round != null && tr.Round.Sequence > 1))
            {
                TempData["ErrorMessage"] = "Cannot create first round. Subsequent rounds already exist for this division. Please edit Round 1 instead.";
                
                // Find CurrentRound 1 to redirect to edit
                var firstRound = existingRounds.FirstOrDefault(tr => tr.Round != null && tr.Round.Sequence == 1);
                if (firstRound != null)
                {
                    return RedirectToAction(nameof(Edit), new { id = firstRound.Id });
                }
                
                return RedirectToAction(nameof(Index), new { tournamentId, divisionId });
            }

            // Check if CurrentRound 1 already exists
            var round1 = existingRounds.FirstOrDefault(tr => tr.Round != null && tr.Round.Sequence == 1);
            if (round1 != null)
            {
                TempData["ErrorMessage"] = "First round already exists. Redirecting to edit.";
                return RedirectToAction(nameof(Edit), new { id = round1.Id });
            }

            var tournament = await _context.Tournaments.FindAsync(tournamentId.Value);
            var division = await _context.Divisions.FindAsync(divisionId.Value);
            
            // Get CurrentRound with RoundNumber = 1
            var firstRoundDef = await _context.RoundTemplates.AsNoTracking().FirstOrDefaultAsync(r => r.Sequence == 1);
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

            // Create the tournament tournamentRound
            var tournamentRound = await _tournamentRoundService.CreateFirstRoundAsync(
                model.TournamentId,
                model.DivisionId,
                model.RoundId,
                model.AdvancingTeamsCount,
                model.AdvancingTeamSelectionStrategy,
                model.MatchGenerationStrategy,
                model.GroupConfigurationType,
                model.GroupConfigurationValue,
                userName);

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
            var tournamentRound = await _tournamentRoundService.GetTournamentRoundByIdAsync(id);
            if (tournamentRound == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments.FindAsync(tournamentRound.TournamentId);
            var division = await _context.Divisions.FindAsync(tournamentRound.DivisionId);
            
            // Count teams in division
            var teamsCount = await _context.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == tournamentRound.TournamentId && ttd.DivisionId == tournamentRound.DivisionId)
                .CountAsync();

            var hasTeams = await _tournamentRoundService.HasTeamsAssignedAsync(id);
            var hasMatches = await _tournamentRoundService.HasMatchesGeneratedAsync(id);

            // Determine group configuration type and value
            GroupGenerationStrategy groupConfigType = tournamentRound.GroupingStrategy;
            int groupConfigValue = 2;
            
            if (tournamentRound.GroupsInRound.HasValue && tournamentRound.GroupsInRound.Value > 0)
            {
                groupConfigType = GroupGenerationStrategy.GroupsInRound;
                groupConfigValue = tournamentRound.GroupsInRound.Value;
            }
            else if (tournamentRound.TeamsPerGroup.HasValue && tournamentRound.TeamsPerGroup.Value > 0)
            {
                groupConfigType = GroupGenerationStrategy.TeamsPerGroup;
                groupConfigValue = tournamentRound.TeamsPerGroup.Value;
            }

            var model = new EditTournamentRoundViewModel
            {
                Id = tournamentRound.Id,
                TournamentId = tournamentRound.TournamentId,
                DivisionId = tournamentRound.DivisionId,
                RoundId = tournamentRound.RoundTemplateId,
                TournamentName = tournament?.Name ?? "Unknown",
                DivisionName = division?.Name ?? "Unknown",
                RoundName = tournamentRound.Round?.Name ?? $"Round {tournamentRound.RoundNumber}",
                TotalTeamsInDivision = teamsCount,
                RoundNumber = tournamentRound.RoundNumber,
                AdvancingTeamSelectionStrategy = tournamentRound.AdvancingTeamSelectionStrategy,
                MatchGenerationStrategy = tournamentRound.MatchGenerationStrategy,
                AdvancingTeamsCount = tournamentRound.AdvancingTeamsCount,
                QualifyingTeamsCount = tournamentRound.QualifyingTeamsCount,
                QualifyingTeamSelectionStrategy = tournamentRound.QualifyingTeamSelectionStrategy,
                GroupConfigurationType = groupConfigType,
                GroupConfigurationValue = groupConfigValue,
                IsFinished = tournamentRound.IsFinished,
                IsLocked = tournamentRound.IsLocked,
                IsPlayoff = tournamentRound.IsPlayoff,
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

            // Update tournamentRound properties
            tournamentRound.AdvancingTeamSelectionStrategy = model.AdvancingTeamSelectionStrategy;
            tournamentRound.AdvancingTeamsCount = model.AdvancingTeamsCount;
            tournamentRound.MatchGenerationStrategy = model.MatchGenerationStrategy;
            tournamentRound.QualifyingTeamsCount = model.QualifyingTeamsCount;
            tournamentRound.QualifyingTeamSelectionStrategy = model.QualifyingTeamSelectionStrategy;
            
            // Update group configuration
            tournamentRound.GroupingStrategy = model.GroupConfigurationType;
            tournamentRound.TeamsPerGroup = model.GroupConfigurationType == GroupGenerationStrategy.TeamsPerGroup ? model.GroupConfigurationValue : null;
            tournamentRound.GroupsInRound = model.GroupConfigurationType == GroupGenerationStrategy.GroupsInRound ? model.GroupConfigurationValue : null;
            
            tournamentRound.UpdatedAt = DateTime.Now;
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

            // Get all rounds and the current tournamentRound to disable it in the dropdown
            var allRounds = await _context.RoundTemplates.AsNoTracking().OrderBy(r => r.Sequence).ToListAsync();
            ViewData["Rounds"] = new SelectList(allRounds, "Id", "Name");

            // Get team count from current tournamentRound
            var currentTeamCount = await _tournamentRoundService.GetRoundTeamsAsync(id);

            var model = new CreateNextRoundViewModel
            {
                TournamentId = currentRound.TournamentId,
                DivisionId = currentRound.DivisionId,
                PreviousTournamentRoundId = id,
                CurrentRoundId = currentRound.RoundTemplateId,
                TournamentName = tournament?.Name ?? "Unknown",
                DivisionName = division?.Name ?? "Unknown",
                PreviousRoundName = currentRound.Round?.Name ?? $"Round {currentRound.RoundNumber}",
                
                // QUALIFYING: Teams coming into this NEW round (based on current round's advancing settings)
                QualifyingTeamsCount = currentRound.AdvancingTeamsCount,
                QualifyingTeamSelectionStrategy = currentRound.AdvancingTeamSelectionStrategy,
                SourceMatchStrategy = currentRound.MatchGenerationStrategy,
                
                // ADVANCING: Default settings for teams advancing from the NEW round
                AdvancingTeamsCount = Math.Max(2, currentRound.AdvancingTeamsCount / 2), // Default to half, minimum 2
                AdvancingTeamSelectionStrategy = currentRound.AdvancingTeamSelectionStrategy, // Same as current
                MatchGenerationStrategy = MatchGenerationStrategy.SeededBracket,
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
                var rounds = await _context.RoundTemplates.OrderBy(r => r.Sequence).ToListAsync();
                ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");
                return View(model);
            }

            var userName = User.Identity?.Name ?? "admin";

            // Create the next tournament tournamentRound
            var tournamentRound = await _tournamentRoundService.CreateNextRoundAsync(
                model.TournamentId,
                model.DivisionId,
                model.RoundId,
                model.PreviousTournamentRoundId,
                model.QualifyingTeamsCount,
                model.QualifyingTeamSelectionStrategy,
                model.AdvancingTeamsCount,
                model.AdvancingTeamSelectionStrategy,
                model.MatchGenerationStrategy,
                model.GroupConfigurationType,
                model.GroupConfigurationValue,
                false,
                userName);

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
            
            var rounds = await _context.RoundTemplates.OrderBy(r => r.Sequence).ToListAsync();
            ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");
            return View(model);
        }
    }

    // GET: Admin/TournamentRounds/CreateNextRound/5
    public async Task<IActionResult> CreateNextRound(Guid id)
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
                TempData["ErrorMessage"] = "Current round must be finalized before creating next round.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var tournament = await _context.Tournaments.FindAsync(currentRound.TournamentId);
            var division = await _context.Divisions.FindAsync(currentRound.DivisionId);

            // Get only non-playoff rounds for dropdown
            var nonPlayoffRounds = await _context.RoundTemplates
                .Where(r => !r.IsPlayoff)
                .OrderBy(r => r.Sequence)
                .ToListAsync();
            ViewData["Rounds"] = new SelectList(nonPlayoffRounds, "Id", "Name");

            var model = new CreateNextRoundSimpleViewModel
            {
                TournamentId = currentRound.TournamentId,
                DivisionId = currentRound.DivisionId,
                PreviousTournamentRoundId = id,
                TournamentName = tournament?.Name ?? "Unknown",
                DivisionName = division?.Name ?? "Unknown",
                PreviousRoundName = currentRound.Round?.Name ?? $"Round {currentRound.RoundNumber}",
                PreviousRoundAdvancingTeams = currentRound.AdvancingTeamsCount
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create next round form");
            TempData["ErrorMessage"] = "Error loading form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Admin/TournamentRounds/CreateNextRound
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNextRound(CreateNextRoundSimpleViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var rounds = await _context.RoundTemplates
                    .Where(r => !r.IsPlayoff)
                    .OrderBy(r => r.Sequence)
                    .ToListAsync();
                ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");
                return View(model);
            }

            var userName = User.Identity?.Name ?? "admin";
            
            // Get the selected RoundTemplate template to retrieve recommendations
            var selectedRound = await _context.RoundTemplates.FindAsync(model.RoundId);
            if (selectedRound == null)
            {
                ModelState.AddModelError("", "Selected round template not found.");
                var rounds = await _context.RoundTemplates
                    .Where(r => !r.IsPlayoff)
                    .OrderBy(r => r.Sequence)
                    .ToListAsync();
                ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");
                return View(model);
            }
            
            // Get previous round to calculate defaults
            var previousRound = await _tournamentRoundService.GetTournamentRoundByIdAsync(model.PreviousTournamentRoundId);
            if (previousRound == null)
            {
                ModelState.AddModelError("", "Previous round not found.");
                var rounds = await _context.RoundTemplates
                    .Where(r => !r.IsPlayoff)
                    .OrderBy(r => r.Sequence)
                    .ToListAsync();
                ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");
                return View(model);
            }

            // Calculate defaults from RoundTemplate template and previous round
            int qualifyingTeamsCount = selectedRound.RecommendedQualifyingTeamsCount > 0
                ? selectedRound.RecommendedQualifyingTeamsCount
                : previousRound.AdvancingTeamsCount;
                
            int advancingTeamsCount = Math.Max(2, qualifyingTeamsCount / 2);
            
            // Determine group configuration based on match strategy
            GroupGenerationStrategy groupStrategy = GroupGenerationStrategy.NoGroup;
            int groupValue = 2;
            
            if (selectedRound.RecommendedMatchGenerationStrategy == MatchGenerationStrategy.GroupStageKnockout)
            {
                groupStrategy = GroupGenerationStrategy.GroupsInRound;
                groupValue = Math.Max(2, qualifyingTeamsCount / 4);
            }

            // Create the tournament round using the service
            var tournamentRound = await _tournamentRoundService.CreateNextRoundAsync(
                model.TournamentId,
                model.DivisionId,
                model.RoundId,
                model.PreviousTournamentRoundId,
                qualifyingTeamsCount,
                selectedRound.RecommendedTeamSelectionStrategy,
                advancingTeamsCount,
                selectedRound.RecommendedTeamSelectionStrategy,
                selectedRound.RecommendedMatchGenerationStrategy,
                groupStrategy,
                groupValue,
                selectedRound.IsPlayoff,
                userName);

            TempData["SuccessMessage"] = $"Round '{selectedRound.Name}' created successfully with recommended defaults. You can customize settings below.";
            
            // Redirect to Edit page for further customization
            return RedirectToAction(nameof(Edit), new { id = tournamentRound.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating next round");
            ModelState.AddModelError("", $"Error creating next round: {ex.Message}");
            
            var rounds = await _context.RoundTemplates
                .Where(r => !r.IsPlayoff)
                .OrderBy(r => r.Sequence)
                .ToListAsync();
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
            
            // Auto-assign teams for round 1 if not yet assigned
            List<TournamentRoundTeam> teams;
            if (round.RoundNumber == 1)
            {
                teams = await _tournamentRoundService.GetRoundTeamsAsync(id);
                if (!teams.Any())
                {
                    var userName = User.Identity?.Name ?? "admin";
                    teams = await _tournamentRoundService.AssignFirstRoundTeamsAsync(id, userName);
                    TempData["InfoMessage"] = "Teams were automatically assigned for Round 1.";
                }
            }
            else
            {
                teams = await _tournamentRoundService.GetRoundTeamsAsync(id);
            }

            var model = new GenerateMatchesViewModel
            {
                TournamentRoundId = id,
                RoundName = round.Round?.Name ?? $"Round {round.RoundNumber}",
                StartTime = await _activeTournamentService.GetNextMatchStartTimeAsync(),
                StartingCourtNumber = 1,
                NumberOfCourts = 3,
                MatchTimeInterval = 10,
                TeamCount = teams.Count,
                Strategy = round.MatchGenerationStrategy
            };
            int dGroups = teams.Select(t => t.GroupName).Distinct().Count();
            if (dGroups > 1 && dGroups < 7)
            {
                model.NumberOfCourts = dGroups;
            }

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
            
            // Redirect to tournamentRound details
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
            //Done inside finalize
            //var rankedTeams = await _ranksService.UpdateTeamRanksAsync(id);
            var round = await _tournamentRoundService.FinalizeRoundAsync(id, userName);
            await _tournamentChannel.UpdateDivisionRanksAsync(round.TournamentId, round.DivisionId);

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
    // POST: Admin/TournamentRounds/FinalizeRound/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnfinalizeRound(Guid id)
    {
        try
        {
            var userName = User.Identity?.Name ?? "admin";
            var round = await _tournamentRoundService.UnfinalizeRoundAsync(id, userName);
            var rankedTeams = await _ranksService.UpdateTeamRanksAsync(id);

            TempData["SuccessMessage"] = "Round unlocked successfully. Team rankings have been calculated.";
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

            if (round.IsLocked)
            {
                TempData["ErrorMessage"] = "Cannot rank teams for a locked round.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var rankedTeams = await _ranksService.UpdateTeamRanksAsync(id);
            await _tournamentChannel.UpdateDivisionRanksAsync(round.TournamentId, round.DivisionId);
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

    // GET: Admin/TournamentRounds/CreatePlayoffRound/5
    public async Task<IActionResult> CreatePlayoffRound(Guid id)
    {
        try
        {
            var previousRound = await _tournamentRoundService.GetTournamentRoundByIdAsync(id);
            if (previousRound == null)
            {
                return NotFound();
            }

            if (!previousRound.IsFinished)
            {
                TempData["ErrorMessage"] = "Previous round must be finalized before creating a playoff round.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (previousRound.IsPlayoff)
            {
                TempData["ErrorMessage"] = "Cannot create a playoff round from another playoff round.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var tournament = await _context.Tournaments.FindAsync(previousRound.TournamentId);
            var division = await _context.Divisions.FindAsync(previousRound.DivisionId);

            // Get all rounds for the dropdown
            var allRounds = await _context.RoundTemplates.OrderBy(r => r.Sequence).ToListAsync();
            ViewData["Rounds"] = new SelectList(allRounds, "Id", "Name");

            // Suggest a default number of teams (e.g., half the teams that didn't advance, minimum 4)
            var teamsInPreviousRound = await _tournamentRoundService.GetRoundTeamsAsync(id);
            int defaultTeamCount = Math.Max(4, (teamsInPreviousRound.Count - previousRound.AdvancingTeamsCount) / 2);

            // Get playoff candidate teams
            var candidateTeams = await _tournamentRoundService.GetPlayoffCandidateTeamsAsync(id, defaultTeamCount);

            var model = new CreatePlayoffRoundViewModel
            {
                TournamentId = previousRound.TournamentId,
                DivisionId = previousRound.DivisionId,
                PreviousRoundId = id,
                TournamentName = tournament?.Name ?? "Unknown",
                DivisionName = division?.Name ?? "Unknown",
                PreviousRoundName = previousRound.Round?.Name ?? $"Round {previousRound.RoundNumber}",
                RoundName = "Playoff Round",
                NumberOfTeamsToSelect = defaultTeamCount,
                CandidateTeams = candidateTeams.ToList(),
                SelectedTeamIds = candidateTeams.Select(t => t.TeamId).ToList(),
                
                // Default settings for the playoff round
                AdvancingTeamsCount = Math.Max(2, defaultTeamCount / 2),
                AdvancingTeamSelectionStrategy = TeamSelectionStrategy.TopByPoints,
                MatchStrategy = MatchGenerationStrategy.SeededBracket,
                GroupConfigurationType = GroupGenerationStrategy.NoGroup,
                GroupConfigurationValue = 2,
                
                // Default immediate actions
                AssignTeamsNow = true,
                GenerateMatchesNow = true
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create playoff round form for round {RoundId}", id);
            TempData["ErrorMessage"] = "Error loading form.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // POST: Admin/TournamentRounds/CreatePlayoffRound
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePlayoffRound(CreatePlayoffRoundViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var rounds = await _context.RoundTemplates.OrderBy(r => r.Sequence).ToListAsync();
                ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");
                
                // Reload candidate teams if validation fails
                model.CandidateTeams = (await _tournamentRoundService.GetPlayoffCandidateTeamsAsync(
                    model.PreviousRoundId,
                    model.NumberOfTeamsToSelect)).ToList();
                
                return View(model);
            }

            var userName = User.Identity?.Name ?? "admin";
            var previousRound = await _tournamentRoundService.GetTournamentRoundByIdAsync(model.PreviousRoundId);
            
            if (previousRound == null)
            {
                TempData["ErrorMessage"] = "Previous round not found.";
                return RedirectToAction(nameof(Index));
            }

            // Create the playoff tournament round
            var playoffRound = await _tournamentRoundService.CreateNextRoundAsync(
                model.TournamentId,
                model.DivisionId,
                model.RoundId,
                model.PreviousRoundId,
                model.SelectedTeamIds.Count,
                TeamSelectionStrategy.Manual,
                0,
                TeamSelectionStrategy.Manual,
                model.MatchStrategy,
                model.GroupConfigurationType,
                model.GroupConfigurationValue,
                true,
                userName);

            // Manually add the selected teams to the playoff round
            int seedNumber = 1;
            foreach (var teamId in model.SelectedTeamIds)
            {
                var roundTeam = new TournamentRoundTeam
                {
                    TournamentId = playoffRound.TournamentId,
                    DivisionId = playoffRound.DivisionId,
                    RoundTemplateId = playoffRound.RoundTemplateId,
                    TeamId = teamId,
                    TournamentRoundId = playoffRound.Id,
                    SeedNumber = seedNumber++,
                    GroupName = string.Empty,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    CreatedAt = DateTime.Now
                };

                _context.TournamentRoundTeams.Add(roundTeam);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Playoff round created successfully with {model.SelectedTeamIds.Count} teams.";
            
            // Immediate execution: Generate matches if requested
            if (model.GenerateMatchesNow)
            {
                return RedirectToAction(nameof(GenerateMatches), new { id = playoffRound.Id });
            }
            
            return RedirectToAction(nameof(Details), new { id = playoffRound.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating playoff round");
            ModelState.AddModelError("", $"Error creating playoff round: {ex.Message}");
            
            var rounds = await _context.RoundTemplates.OrderBy(r => r.Sequence).ToListAsync();
            ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");
            
            // Reload candidate teams on error
            try
            {
                model.CandidateTeams = (await _tournamentRoundService.GetPlayoffCandidateTeamsAsync(
                    model.PreviousRoundId,
                    model.NumberOfTeamsToSelect)).ToList();
            }
            catch
            {
                model.CandidateTeams = new List<TournamentRoundTeamSummaryViewModel>();
            }
            
            return View(model);
        }
    }
}
