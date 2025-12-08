using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.App.Areas.Admin.Models;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Services;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TournamentTeamsController : Controller
    {
        private readonly IActiveTournamentService _activeTournamentService;
        private readonly ITournamentService _tournamentService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TournamentTeamsController> _logger;

        public TournamentTeamsController(
            IActiveTournamentService activeTournamentService, 
            ITournamentService tournamentService,
            ApplicationDbContext context,
            ILogger<TournamentTeamsController> logger)
        {
            _activeTournamentService = activeTournamentService;
            _tournamentService = tournamentService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(Guid? divisionId)
        {
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }

            if (divisionId.HasValue && divisionId.Value != Guid.Empty)
            {
                /*
                var division = await _tournamentService.GetDivisions .FirstOrDefault(d => d.DivisionId == divisionId);
                if (division == null)
                {
                    return NotFound("Division not found in the active tournament.");
                }*/
            }
            var model = await _activeTournamentService.GetTournamentTeamsAsync(Guid.Empty);
            model = model.OrderBy(tt => tt.SeedNumber).ThenBy(tt => tt.Team.Name).ToList();

            //model = model.OrderBy(ttd => ttd.GroupName);
            ViewBag.Divisions = await _activeTournamentService.GetTournamentDivisionsAsync();
            return View(model);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
                if (activeTournament == null)
                {
                    _logger.LogWarning("No active tournament found.");
                    return NotFound("No active tournament found.");
                }

                var tournamentTeamDivision = await _activeTournamentService.GetTeamAsync(id);
                if (tournamentTeamDivision == null)
                {
                    _logger.LogWarning("Team with ID {TeamId} not found.", id);
                    return NotFound();
                }

                // Get all matches for this team
                var matches = await _activeTournamentService.GetMatchesAsync(teamId: id);

                var model = new TournamentTeamDetailsViewModel
                {
                    TournamentTeamDivision = tournamentTeamDivision,
                    Matches = matches.OrderByDescending(m => m.ScheduledTime).ToList(),
                    Division = tournamentTeamDivision.Division
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team details for team {TeamId}", id);
                return StatusCode(500, "An error occurred while retrieving team details.");
            }
        }

        public async Task<IActionResult> Create()
        {
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }

            var model = new TournamentTeamAddViewModel()
            {
                TournamentId = activeTournament.Id,
                ActiveTournament = activeTournament,
                AvailableTeams = await _activeTournamentService.GetAvailableTeamsAsync(),
                AvailableDivisions = await _activeTournamentService.GetAvailableDivisionsAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamId,DivisionId,TournamentId,GroupName,SeedNumber,Rating")] TournamentTeamAddViewModel tournamentTeamDivision)
        {
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }
            if (activeTournament.Id != tournamentTeamDivision.TournamentId)
            {
                return NotFound("Active tournament has changed.");
            }

            var division = activeTournament.TournamentDivisions.FirstOrDefault(d => d.DivisionId == tournamentTeamDivision.DivisionId);
            if (division == null)
            {
                return NotFound("Division not found in the active tournament.");
            }

            if (ModelState.IsValid)
            {
                await _activeTournamentService.AddTeamAsync(tournamentTeamDivision.TeamId, tournamentTeamDivision.DivisionId, tournamentTeamDivision.GroupName ?? "", tournamentTeamDivision.SeedNumber, tournamentTeamDivision.Rating);
                return RedirectToAction(nameof(Index));
            }
            tournamentTeamDivision.TournamentId = activeTournament.Id;
            tournamentTeamDivision.ActiveTournament = activeTournament;
            tournamentTeamDivision.AvailableTeams = await _activeTournamentService.GetAvailableTeamsAsync();
            tournamentTeamDivision.AvailableDivisions = await _activeTournamentService.GetAvailableDivisionsAsync();
            return View(tournamentTeamDivision);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }

            var tournamentTeamDivision = await _activeTournamentService.GetTeamAsync(id);
            if (tournamentTeamDivision == null)
            {
                return NotFound();
            }
            var model = new TournamentTeamAddViewModel()
            {
                TeamName = tournamentTeamDivision.Team.Name,
                TournamentId = activeTournament.Id,
                ActiveTournament = activeTournament,
                AvailableTeams = await _activeTournamentService.GetAvailableTeamsAsync(),
                AvailableDivisions = await _activeTournamentService.GetAvailableDivisionsAsync(),
                TeamId = tournamentTeamDivision.TeamId,
                DivisionId = tournamentTeamDivision.DivisionId,
                GroupName = tournamentTeamDivision.GroupName,
                SeedNumber = tournamentTeamDivision.SeedNumber,
                Rating = tournamentTeamDivision.Rating
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("TeamId,DivisionId,TournamentId,GroupName,SeedNumber,Rating")] TournamentTeamAddViewModel tournamentTeamDivision)
        {
            if (id != tournamentTeamDivision.TeamId)
            {
                return NotFound();
            }

            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }

            var division = activeTournament.TournamentDivisions.FirstOrDefault(d => d.DivisionId == tournamentTeamDivision.DivisionId);
            if (division == null)
            {
                return NotFound("Division not found in the active tournament.");
            }
            var existingTeamDivision = await _activeTournamentService.GetTeamAsync(tournamentTeamDivision.TeamId);


            if (ModelState.IsValid)
            {
                if (existingTeamDivision != null)
                {
                    await _activeTournamentService.SetTeamAsync(tournamentTeamDivision.TeamId, tournamentTeamDivision.DivisionId, tournamentTeamDivision.GroupName ?? "", tournamentTeamDivision.SeedNumber, tournamentTeamDivision.Rating);
                    return RedirectToAction(nameof(Index));
                }
                return NotFound();
            }
            if (existingTeamDivision == null)
            {
                return NotFound();
            }

            var model = new TournamentTeamAddViewModel()
            {
                TeamName = existingTeamDivision.Team.Name,
                TournamentId = activeTournament.Id,
                ActiveTournament = activeTournament,
                AvailableTeams = await _activeTournamentService.GetAvailableTeamsAsync(),
                AvailableDivisions = await _activeTournamentService.GetAvailableDivisionsAsync(),
                TeamId = tournamentTeamDivision.TeamId,
                DivisionId = tournamentTeamDivision.DivisionId,
                GroupName = tournamentTeamDivision.GroupName,
                SeedNumber = tournamentTeamDivision.SeedNumber,
                Rating = tournamentTeamDivision.Rating
            };
            return View(model);
        }

        public async Task<IActionResult> Delete(Guid id, Guid divisionId)
        {
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }
            var tournamentTeamDivision = await _activeTournamentService.GetTeamAsync(id);
            if (tournamentTeamDivision == null)
            {
                return NotFound();
            }
            return View(tournamentTeamDivision);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, Guid divisionId)
        {
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }

            await _activeTournamentService.RemoveTeamAsync(id);
            return RedirectToAction(nameof(Index), new { divisionId });
        }

        public async Task<IActionResult> BulkAssign()
        {
            try
            {
                var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
                if (activeTournament == null)
                {
                    _logger.LogWarning("No active tournament found.");
                    return NotFound("No active tournament found.");
                }

                // Get all teams
                var allTeams = await _activeTournamentService.GetAvailableTeamsAsync();
                
                // Get currently assigned teams
                var assignedTeams = await _activeTournamentService.GetTournamentTeamsAsync(Guid.Empty);
                
                // Create a dictionary for quick lookup
                var assignedTeamsDict = assignedTeams.ToDictionary(t => t.TeamId);

                // Build the team assignment items
                var teamItems = allTeams.Select(team => new TeamAssignmentItem
                {
                    TeamId = team.Id,
                    TeamName = team.Name,
                    School = team.School,
                    CurrentDivisionId = assignedTeamsDict.ContainsKey(team.Id) ? assignedTeamsDict[team.Id].DivisionId : null,
                    CurrentDivisionName = assignedTeamsDict.ContainsKey(team.Id) ? assignedTeamsDict[team.Id].Division.Name : null,
                    CurrentSeedNumber = assignedTeamsDict.ContainsKey(team.Id) ? assignedTeamsDict[team.Id].SeedNumber : 0,
                    CurrentRating = assignedTeamsDict.ContainsKey(team.Id) ? assignedTeamsDict[team.Id].Rating : 0,
                    IsAssigned = assignedTeamsDict.ContainsKey(team.Id)
                }).OrderBy(t => t.TeamName);

                var model = new BulkTeamAssignmentViewModel
                {
                    ActiveTournament = activeTournament,
                    AvailableDivisions = await _activeTournamentService.GetAvailableDivisionsAsync(),
                    Teams = teamItems
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bulk assignment view.");
                return StatusCode(500, "An error occurred while loading the bulk assignment view.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkAssignUpdate([FromBody] BulkTeamAssignmentUpdateModel model)
        {
            try
            {
                var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
                if (activeTournament == null)
                {
                    _logger.LogWarning("No active tournament found.");
                    return NotFound(new { success = false, message = "No active tournament found." });
                }

                // Check if team is currently assigned
                var existingTeam = await _activeTournamentService.GetTeamAsync(model.TeamId);

                if (model.DivisionId.HasValue && model.DivisionId.Value != Guid.Empty)
                {
                    // Assign or update team
                    if (existingTeam != null)
                    {
                        // Update existing assignment
                        await _activeTournamentService.SetTeamAsync(model.TeamId, model.DivisionId.Value, "", model.SeedNumber, model.Rating);
                        _logger.LogInformation("Updated team {TeamId} to division {DivisionId} with seed {SeedNumber}", 
                            model.TeamId, model.DivisionId.Value, model.SeedNumber);
                    }
                    else
                    {
                        // Add new assignment
                        await _activeTournamentService.AddTeamAsync(model.TeamId, model.DivisionId.Value, "", model.SeedNumber, model.Rating);
                        _logger.LogInformation("Added team {TeamId} to division {DivisionId} with seed {SeedNumber}", 
                            model.TeamId, model.DivisionId.Value, model.SeedNumber);
                    }
                    
                    return Ok(new { success = true, message = "Team assignment updated successfully." });
                }
                else
                {
                    // Remove team from tournament
                    if (existingTeam != null)
                    {
                        await _activeTournamentService.RemoveTeamAsync(model.TeamId);
                        _logger.LogInformation("Removed team {TeamId} from tournament", model.TeamId);
                    }
                    
                    return Ok(new { success = true, message = "Team removed from tournament successfully." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating team assignment for team {TeamId}", model.TeamId);
                return StatusCode(500, new { success = false, message = "An error occurred while updating team assignment." });
            }
        }

        public async Task<IActionResult> AutoSeed()
        {
            try
            {
                var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
                if (activeTournament == null)
                {
                    _logger.LogWarning("No active tournament found.");
                    return NotFound("No active tournament found.");
                }

                var model = new AutoSeedViewModel
                {
                    ActiveTournament = activeTournament,
                    AvailableDivisions = (await _activeTournamentService.GetAvailableDivisionsAsync()).ToList(),
                    SeedingMethod = SeedingMethod.SeedUnseeded,
                    SortingMethod = SortingMethod.ByCreationDate,
                    SeedPlacement = SeedPlacement.FillGaps,
                    GapClosure = SeedGapClosure.LetGapsRemain
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading AutoSeed view.");
                return StatusCode(500, "An error occurred while loading the AutoSeed view.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoSeed(AutoSeedViewModel model)
        {
            try
            {
                var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
                if (activeTournament == null)
                {
                    _logger.LogWarning("No active tournament found.");
                    return NotFound("No active tournament found.");
                }

                if (!ModelState.IsValid)
                {
                    model.ActiveTournament = activeTournament;
                    model.AvailableDivisions = (await _activeTournamentService.GetAvailableDivisionsAsync()).ToList();
                    return View(model);
                }

                // Determine which divisions to process
                var divisionIds = new List<Guid>();
                if (model.SelectedDivisionId.HasValue && model.SelectedDivisionId.Value != Guid.Empty)
                {
                    divisionIds.Add(model.SelectedDivisionId.Value);
                }
                else
                {
                    // All divisions
                    var divisions = await _activeTournamentService.GetAvailableDivisionsAsync();
                    divisionIds.AddRange(divisions.Select(d => d.Id));
                }

                // Execute seeding within a transaction
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var divisionId in divisionIds)
                        {
                            await ProcessDivisionSeeding(activeTournament.Id, divisionId, model);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("AutoSeed completed successfully for {DivisionCount} division(s)", divisionIds.Count);
                        TempData["SuccessMessage"] = $"Teams seeded successfully in {divisionIds.Count} division(s).";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing AutoSeed.");
                TempData["ErrorMessage"] = "An error occurred while seeding teams. Please try again.";
                
                model.ActiveTournament = await _activeTournamentService.GetActiveTournamentAsync();
                model.AvailableDivisions = (await _activeTournamentService.GetAvailableDivisionsAsync()).ToList();
                return View(model);
            }
        }

        private async Task ProcessDivisionSeeding(Guid tournamentId, Guid divisionId, AutoSeedViewModel model)
        {
            if (model.SeedingMethod == SeedingMethod.ReseedAll)
            {
                await ReseedAllTeams(tournamentId, divisionId, model);
            }
            else
            {
                await SeedUnseededTeams(tournamentId, divisionId, model);
            }

            // Apply gap closure if requested
            if (model.GapClosure == SeedGapClosure.CloseAllGaps)
            {
                await CloseAllGaps(tournamentId, divisionId, model.SortingMethod);
            }
        }

        private async Task ReseedAllTeams(Guid tournamentId, Guid divisionId, AutoSeedViewModel model)
        {
            // Get all teams in the division
            var teams = await _context.TournamentTeamDivisions
                .Include(t => t.Team)
                .Where(t => t.TournamentId == tournamentId && t.DivisionId == divisionId)
                .ToListAsync();

            // Sort teams based on sorting method
            var sortedTeams = ApplySorting(teams, model.SortingMethod);

            // Assign sequential seed numbers starting from 1
            int seedNumber = 1;
            foreach (var team in sortedTeams)
            {
                team.SeedNumber = seedNumber++;
            }

            _logger.LogInformation("Reseeded {TeamCount} teams in division {DivisionId}", teams.Count, divisionId);
        }

        private async Task SeedUnseededTeams(Guid tournamentId, Guid divisionId, AutoSeedViewModel model)
        {
            // Get unseeded teams (SeedNumber is 0 or null)
            var unseededTeams = await _context.TournamentTeamDivisions
                .Include(t => t.Team)
                .Where(t => t.TournamentId == tournamentId && 
                           t.DivisionId == divisionId && 
                           t.SeedNumber <= 0)
                .ToListAsync();

            if (!unseededTeams.Any())
            {
                _logger.LogInformation("No unseeded teams found in division {DivisionId}", divisionId);
                return;
            }

            // Get already seeded teams
            var seededTeams = await _context.TournamentTeamDivisions
                .Where(t => t.TournamentId == tournamentId && 
                           t.DivisionId == divisionId && 
                           t.SeedNumber > 0)
                .Select(t => t.SeedNumber)
                .ToListAsync();

            var seededNumbers = new HashSet<int>(seededTeams);

            // Sort unseeded teams
            var sortedUnseededTeams = ApplySorting(unseededTeams, model.SortingMethod);

            // Determine starting seed number
            int currentSeed;
            if (model.SeedPlacement == SeedPlacement.AtTheEnd)
            {
                // Start after the max existing seed
                currentSeed = seededNumbers.Any() ? seededNumbers.Max() + 1 : 1;
                
                foreach (var team in sortedUnseededTeams)
                {
                    team.SeedNumber = currentSeed++;
                }
            }
            else // Fill Gaps
            {
                currentSeed = 1;
                
                foreach (var team in sortedUnseededTeams)
                {
                    // Find next available seed number
                    while (seededNumbers.Contains(currentSeed))
                    {
                        currentSeed++;
                    }
                    
                    team.SeedNumber = currentSeed;
                    seededNumbers.Add(currentSeed);
                    currentSeed++;
                }
            }

            _logger.LogInformation("Seeded {TeamCount} unseeded teams in division {DivisionId}", unseededTeams.Count, divisionId);
        }

        private async Task CloseAllGaps(Guid tournamentId, Guid divisionId, SortingMethod sortingMethod)
        {
            // Get all seeded teams
            var seededTeams = await _context.TournamentTeamDivisions
                .Include(t => t.Team)
                .Where(t => t.TournamentId == tournamentId && 
                           t.DivisionId == divisionId && 
                           t.SeedNumber > 0)
                .ToListAsync();

            if (!seededTeams.Any())
            {
                return;
            }

            // Sort primarily by current seed number, then by sorting method as tiebreaker
            var sortedTeams = seededTeams
                .OrderBy(t => t.SeedNumber)
                .ThenBy(t => GetSortingKey(t, sortingMethod))
                .ToList();

            // Re-assign sequential seed numbers
            int seedNumber = 1;
            foreach (var team in sortedTeams)
            {
                team.SeedNumber = seedNumber++;
            }

            _logger.LogInformation("Closed gaps for {TeamCount} teams in division {DivisionId}", seededTeams.Count, divisionId);
        }

        private List<TournamentTeamDivision> ApplySorting(List<TournamentTeamDivision> teams, SortingMethod method)
        {
            return method switch
            {
                SortingMethod.ByRating => teams.OrderByDescending(t => t.Rating).ToList(),
                SortingMethod.ByCreationDate => teams.OrderBy(t => t.CreatedAt).ToList(),
                SortingMethod.ByTeamName => teams.OrderBy(t => t.Team.Name).ToList(),
                SortingMethod.Randomly => teams.OrderBy(t => Guid.NewGuid()).ToList(),
                _ => teams
            };
        }

        private object GetSortingKey(TournamentTeamDivision team, SortingMethod method)
        {
            return method switch
            {
                SortingMethod.ByCreationDate => team.CreatedAt,
                SortingMethod.ByTeamName => team.Team.Name,
                SortingMethod.Randomly => Guid.NewGuid(),
                _ => team.CreatedAt
            };
        }
    }
}
