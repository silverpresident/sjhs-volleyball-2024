using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.App.Areas.Admin.Models;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MatchesController : Controller
    {

        private readonly IActiveTournamentService _activeTournamentService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMatchService _matchService;

        private readonly ILogger<MatchesController> _logger;

        public MatchesController(ILogger<MatchesController> logger, IActiveTournamentService activeTournamentService, ApplicationDbContext context, IMatchService matchService)
        {
            _dbContext = context;
            _activeTournamentService = activeTournamentService;
            _logger = logger;
            _matchService = matchService;
        }

        // GET: Admin/Matches
        public async Task<IActionResult> Index()
        {
            var matches = await _activeTournamentService.GetMatchesAsync();
            return View(matches);
        }

        // GET: Admin/Matches/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var match = await _matchService.GetMatchAsync(id.Value);
            if (match == null)
            {
                return NotFound();
            }

            var sets = await _matchService.GetMatchSetsAsync(id.Value);
            var updates = await _matchService.GetMatchUpdatesAsync(id.Value);

            var viewModel = new MatchDetailsViewModel
            {
                Match = match,
                Sets = sets,
                Updates = updates
            };

            return View(viewModel);
        }

        // GET: Admin/Matches/Assign/5
        public async Task<IActionResult> Assign(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var match = await _matchService.GetMatchAsync(id.Value);
            if (match == null)
            {
                return NotFound();
            }

            var viewModel = new MatchAssignViewModel
            {
                MatchId = match.Id,
                MatchNumber = match.MatchNumber,
                DivisionName = match.Division?.Name ?? "N/A",
                RoundName = match.Round?.Name ?? "N/A",
                HomeTeamName = match.HomeTeam?.Name ?? "Unknown",
                AwayTeamName = match.AwayTeam?.Name ?? "Unknown",
                ScheduledTime = match.ScheduledTime,
                CourtLocation = match.CourtLocation,
                RefereeName = match.RefereeName,
                ScorerName = match.ScorerName
            };

            return View(viewModel);
        }

        // POST: Admin/Matches/Assign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(Guid id, MatchAssignViewModel model)
        {
            if (id != model.MatchId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _matchService.UpdateMatchDetailsAsync(
                    model.MatchId,
                    model.ScheduledTime,
                    model.CourtLocation,
                    model.RefereeName,
                    model.ScorerName,
                    User.Identity?.Name ?? "admin"
                );

                TempData["SuccessMessage"] = "Match details updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating match: {ex.Message}");
                return View(model);
            }
        }

        // GET: Admin/Matches/Update/5
        public async Task<IActionResult> Update(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var match = await _matchService.GetMatchAsync(id.Value);
            if (match == null)
            {
                return NotFound();
            }

            var viewModel = new MatchUpdateViewModel
            {
                MatchId = match.Id,
                MatchNumber = match.MatchNumber,
                DivisionName = match.Division?.Name ?? "N/A",
                RoundName = match.Round?.Name ?? "N/A",
                CourtLocation = match.CourtLocation ?? "N/A",
                RefereeName = match.RefereeName,
                ScorerName = match.ScorerName,
                HomeTeamName = match.HomeTeam?.Name ?? "Unknown",
                AwayTeamName = match.AwayTeam?.Name ?? "Unknown"
            };

            return View(viewModel);
        }

        // POST: Admin/Matches/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Guid id, MatchUpdateViewModel model)
        {
            if (id != model.MatchId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var update = new MatchUpdate
                {
                    MatchId = model.MatchId,
                    UpdateType = model.UpdateType,
                    Content = model.Comment
                };

                await _matchService.AddMatchUpdateAsync(update);

                TempData["SuccessMessage"] = "Match update added successfully.";
                return RedirectToAction(nameof(Details), new { id = model.MatchId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding match update for match {MatchId}", model.MatchId);
                ModelState.AddModelError("", $"Error adding update: {ex.Message}");
                return View(model);
            }
        }

        // GET: Admin/Matches/Scorer/5
        public async Task<IActionResult> Scorer(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var match = await _matchService.GetMatchAsync(id.Value);
            if (match == null)
            {
                return NotFound();
            }

            var sets = await _matchService.GetMatchSetsAsync(id.Value);
            var recentUpdates = await _matchService.GetMatchUpdatesAsync(id.Value);

            var viewModel = new MatchScorerViewModel
            {
                MatchId = match.Id,
                MatchNumber = match.MatchNumber,
                HomeTeamName = match.HomeTeam?.Name ?? "Unknown",
                HomeTeamId = match.HomeTeamId,
                AwayTeamName = match.AwayTeam?.Name ?? "Unknown",
                AwayTeamId = match.AwayTeamId,
                CourtLocation = match.CourtLocation,
                CurrentSetNumber = match.CurrentSetNumber,
                IsFinished = match.IsFinished,
                IsDisputed = match.IsDisputed,
                ActualStartTime = match.ActualStartTime,
                Sets = sets.Select(s => new MatchSetDto
                {
                    SetNumber = s.SetNumber,
                    HomeTeamScore = s.HomeTeamScore,
                    AwayTeamScore = s.AwayTeamScore,
                    IsFinished = s.IsFinished,
                    IsLocked = s.IsLocked
                }).ToList(),
                RecentUpdates = recentUpdates.Take(15).ToList()
            };

            return View(viewModel);
        }

        // GET: Admin/Matches/Finalize/5
        public async Task<IActionResult> Finalize(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var match = await _matchService.GetMatchAsync(id.Value);
            if (match == null)
            {
                return NotFound();
            }

            var sets = await _matchService.GetMatchSetsAsync(id.Value);

            var viewModel = new MatchFinalizeViewModel
            {
                MatchId = match.Id,
                MatchNumber = match.MatchNumber,
                DivisionName = match.Division?.Name ?? "N/A",
                RoundName = match.Round?.Name ?? "N/A",
                CourtLocation = match.CourtLocation ?? "N/A",
                RefereeName = match.RefereeName,
                ScorerName = match.ScorerName,
                HomeTeamName = match.HomeTeam?.Name ?? "Unknown",
                AwayTeamName = match.AwayTeam?.Name ?? "Unknown",
                HomeTeamScore = match.HomeTeamScore,
                AwayTeamScore = match.AwayTeamScore,
                IsFinished = match.IsFinished,
                IsDisputed = match.IsDisputed,
                IsLocked = match.IsLocked,
                HasSets = sets.Any()
            };

            return View(viewModel);
        }

        // POST: Admin/Matches/Finalize/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalize(Guid id, MatchFinalizeViewModel model)
        {
            if (id != model.MatchId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var match = await _matchService.GetMatchAsync(model.MatchId);
                if (match == null)
                {
                    return NotFound();
                }

                match.IsFinished = model.IsFinished;
                match.IsDisputed = model.IsDisputed;
                match.IsLocked = model.IsLocked;

                // Only update scores if there are no sets
                if (!model.HasSets)
                {
                    match.HomeTeamScore = model.HomeTeamScore;
                    match.AwayTeamScore = model.AwayTeamScore;
                }

                await _matchService.UpdateMatchAsync(match);

                TempData["SuccessMessage"] = "Match finalized successfully.";
                return RedirectToAction(nameof(Details), new { id = model.MatchId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizing match {MatchId}", model.MatchId);
                ModelState.AddModelError("", $"Error finalizing match: {ex.Message}");
                return View(model);
            }
        }

        // GET: Admin/Matches/EditSet/5?setNumber=1
        public async Task<IActionResult> EditSet(Guid? id, int? setNumber)
        {
            if (!id.HasValue || !setNumber.HasValue)
            {
                return NotFound();
            }

            var match = await _matchService.GetMatchAsync(id.Value);
            if (match == null)
            {
                return NotFound();
            }

            var sets = await _matchService.GetMatchSetsAsync(id.Value);
            var set = sets.FirstOrDefault(s => s.SetNumber == setNumber.Value);
            
            if (set == null)
            {
                return NotFound();
            }

            var viewModel = new MatchSetEditViewModel
            {
                MatchId = match.Id,
                SetNumber = set.SetNumber,
                HomeTeamScore = set.HomeTeamScore,
                AwayTeamScore = set.AwayTeamScore,
                IsFinished = set.IsFinished,
                IsLocked = set.IsLocked,
                HomeTeamName = match.HomeTeam?.Name ?? "Unknown",
                AwayTeamName = match.AwayTeam?.Name ?? "Unknown"
            };

            return View(viewModel);
        }

        // POST: Admin/Matches/EditSet/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSet(Guid id, MatchSetEditViewModel model)
        {
            if (id != model.MatchId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var sets = await _matchService.GetMatchSetsAsync(model.MatchId);
                var set = sets.FirstOrDefault(s => s.SetNumber == model.SetNumber);
                
                if (set == null)
                {
                    return NotFound();
                }

                set.HomeTeamScore = model.HomeTeamScore;
                set.AwayTeamScore = model.AwayTeamScore;
                set.IsFinished = model.IsFinished;
                set.IsLocked = model.IsLocked;
                set.UpdatedBy = User.Identity?.Name ?? "admin";
                set.UpdatedAt = DateTime.UtcNow;

                _dbContext.Update(set);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Set updated successfully.";
                return RedirectToAction(nameof(Details), new { id = model.MatchId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating set {SetNumber} for match {MatchId}", model.SetNumber, model.MatchId);
                ModelState.AddModelError("", $"Error updating set: {ex.Message}");
                return View(model);
            }
        }

        // GET: Admin/Matches/Create
        public async Task<IActionResult> Create()
        {
            var tournament = await _activeTournamentService.GetActiveTournamentAsync();
            var teams = await _activeTournamentService.GetAvailableTeamsAsync();
            var rounds = await _dbContext.Rounds.OrderBy(r => r.Sequence).ToListAsync();
            
            // Calculate default scheduled time: 15 minutes after last match or tournament date
            var lastMatch = await _dbContext.Matches
                .Where(m => m.TournamentId == tournament.Id)
                .OrderByDescending(m => m.ScheduledTime)
                .FirstOrDefaultAsync();
            
            DateTime defaultScheduledTime;
            if (lastMatch != null)
            {
                defaultScheduledTime = lastMatch.ScheduledTime.AddMinutes(15);
            }
            else
            {
                defaultScheduledTime = tournament.TournamentDate.AddMinutes(15);
            }

            ViewData["AwayTeamId"] = new SelectList(teams, "Id", "Name");
            ViewData["HomeTeamId"] = new SelectList(teams, "Id", "Name");
            ViewData["Rounds"] = new SelectList(rounds, "Id", "Name");
            ViewData["DefaultScheduledTime"] = defaultScheduledTime.ToString("yyyy-MM-ddTHH:mm");
            
            return View();
        }

        // POST: Admin/Matches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RoundId,HomeTeamId,AwayTeamId,ScheduledTime,CourtLocation,RefereeName,ScorerName,GroupName")] Match match)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var tournament = await _activeTournamentService.GetActiveTournamentAsync();
                    match.TournamentId = tournament.Id;
                    
                    // Get the division from the home team
                    var homeTeam = await _dbContext.TournamentTeamDivisions
                        .FirstOrDefaultAsync(ttd => ttd.TeamId == match.HomeTeamId && ttd.TournamentId == tournament.Id);
                    
                    if (homeTeam != null)
                    {
                        match.DivisionId = homeTeam.DivisionId;
                    }

                    // Get next match number
                    var lastMatchNumber = await _dbContext.Matches
                        .Where(m => m.TournamentId == tournament.Id)
                        .MaxAsync(m => (int?)m.MatchNumber) ?? 0;
                    
                    match.MatchNumber = lastMatchNumber + 1;
                    match.CreatedBy = User.Identity?.Name ?? "admin";
                    match.UpdatedBy = User.Identity?.Name ?? "admin";
                    
                    await _matchService.CreateMatchAsync(match);
                    
                    TempData["SuccessMessage"] = "Match created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating match");
                    ModelState.AddModelError("", $"Error creating match: {ex.Message}");
                }
            }
            
            var tournament2 = await _activeTournamentService.GetActiveTournamentAsync();
            var teams = await _activeTournamentService.GetAvailableTeamsAsync();
            var rounds = await _dbContext.Rounds.OrderBy(r => r.Sequence).ToListAsync();
            
            ViewData["AwayTeamId"] = new SelectList(teams, "Id", "Name", match.AwayTeamId);
            ViewData["HomeTeamId"] = new SelectList(teams, "Id", "Name", match.HomeTeamId);
            ViewData["Rounds"] = new SelectList(rounds, "Id", "Name", match.RoundId);
            
            return View(match);
        }

        // GET: Admin/Matches/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _dbContext.Matches.FindAsync(id);
            if (match == null)
            {
                return NotFound();
            }
            
            var teams = await _activeTournamentService.GetAvailableTeamsAsync();
            var rounds = await _dbContext.Rounds.OrderBy(r => r.Sequence).ToListAsync();
            
            ViewData["AwayTeamId"] = new SelectList(teams, "Id", "Name", match.AwayTeamId);
            ViewData["HomeTeamId"] = new SelectList(teams, "Id", "Name", match.HomeTeamId);
            ViewData["Rounds"] = new SelectList(rounds, "Id", "Name", match.RoundId);
            
            return View(match);
        }

        // POST: Admin/Matches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,RoundId,HomeTeamId,AwayTeamId,ScheduledTime,ActualStartTime,CourtLocation,RefereeName,ScorerName,HomeTeamScore,AwayTeamScore,IsLocked,IsDisputed,IsFinished")] Match match)
        {
            if (id != match.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    match.UpdatedBy = User.Identity?.Name ?? "admin";
                    match.UpdatedAt = DateTime.UtcNow;
                    
                    await _matchService.UpdateMatchAsync(match);
                    TempData["SuccessMessage"] = "Match updated successfully.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!MatchExists(match.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error updating match {MatchId}", match.Id);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating match {MatchId}", match.Id);
                    ModelState.AddModelError("", $"Error updating match: {ex.Message}");
                }
                
                if (ModelState.ErrorCount == 0)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            
            var teams = await _activeTournamentService.GetAvailableTeamsAsync();
            var rounds = await _dbContext.Rounds.OrderBy(r => r.Sequence).ToListAsync();
            
            ViewData["AwayTeamId"] = new SelectList(teams, "Id", "Name", match.AwayTeamId);
            ViewData["HomeTeamId"] = new SelectList(teams, "Id", "Name", match.HomeTeamId);
            ViewData["Rounds"] = new SelectList(rounds, "Id", "Name", match.RoundId);
            
            return View(match);
        }

        // GET: Admin/Matches/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var match = await _dbContext.Matches
                .Include(m => m.AwayTeam)
                .Include(m => m.HomeTeam)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // POST: Admin/Matches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var match = await _dbContext.Matches.FindAsync(id);
            if (match != null)
            {
                _dbContext.Matches.Remove(match);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Matches/AutoGenerateFirstRound
        public async Task<IActionResult> AutoGenerateFirstRound()
        {
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }
            var tds = await _activeTournamentService.GetTournamentDivisionsAsync();
            var divisions = tds.Select(d => new SelectListItem
            {
                Value = d.Division.Id.ToString(),
                Text = d.Division.Name
            }).ToList();

            var viewModel = new AutoGenerateMatchesViewModel
            {
                Divisions = new List<SelectListItem>(),
                // Groups will be populated dynamically based on selected divisions (AJAX or full postback)
                Groups = new List<SelectListItem>()
            };
            viewModel.Divisions.Add(new SelectListItem { Value = Guid.Empty.ToString(), Text = "All Divisions" });
            viewModel.Divisions.AddRange(divisions);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AutoGenerateFirstRound(AutoGenerateMatchesViewModel viewModel)
        {
            // Process the selected divisions and groups
            var selectedDivisionIds = viewModel.SelectedDivisionIds;
            var selectedGroupNames = viewModel.SelectedGroupNames;
            var removeExistingMatches = viewModel.RemoveExistingMatches;

            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }
            // Retrieve teams based on selected divisions and groups

            var qryBaseTTD = _dbContext.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == activeTournament.Id);

            var qry = _dbContext.TournamentTeamDivisions
                .Include(ttd => ttd.Team)
                .Where(ttd => ttd.TournamentId == activeTournament.Id);
            if (selectedDivisionIds.Contains(Guid.Empty) == false && selectedDivisionIds.Count > 0)
            {
                qry = qry.Where(ttd => selectedDivisionIds.Contains(ttd.DivisionId));
            }
            if (selectedGroupNames.Contains(string.Empty) == false && selectedGroupNames.Count > 0)
            {
                qry = qry.Where(ttd => selectedGroupNames.Contains(ttd.GroupName));
            }

            var teams = qry.OrderBy(ttd => ttd.SeedNumber).Select(ttd => ttd.Team).ToList();

            var qryTourMatches = _dbContext.Matches
                .Where(m => m.TournamentId == activeTournament.Id);
            if (selectedDivisionIds.Contains(Guid.Empty) == false && selectedDivisionIds.Count > 0)
            {
                qryTourMatches = qryTourMatches.Where(ttd => selectedDivisionIds.Contains(ttd.DivisionId));
            }
            if (selectedGroupNames.Contains(string.Empty) == false && selectedGroupNames.Count > 0)
            {
                qryTourMatches = qryTourMatches.Where(ttd => selectedGroupNames.Contains(ttd.GroupName));
            }
            // Remove existing matches if requested
            if (removeExistingMatches)
            {
                var qryRM = qryTourMatches;
                //var existingMatches = qryRM;
                _dbContext.Matches.RemoveRange(qryRM);
                await _dbContext.SaveChangesAsync();
            }
            var qryBase = _dbContext.TournamentTeamDivisions
                            .Where(ttd => ttd.TournamentId == activeTournament.Id);
            var matchesToCreate = new List<Match>();
            if (selectedDivisionIds.Contains(Guid.Empty) == true || selectedDivisionIds.Count == 0)
            {
                selectedDivisionIds = qryBase
                .Select(ttd => ttd.DivisionId).Distinct().ToList();
            }

            var nextMatchTime = activeTournament.TournamentDate.AddMinutes(15);

            // Get the "First Round" round
            var firstRound = _dbContext.Rounds.FirstOrDefault(r => r.Name == "Round 1");

            if (firstRound == null)
            {
                return NotFound("First round not found.");
            }
            _logger.LogInformation($"Selected divs: {selectedDivisionIds.Count}");
            Console.WriteLine("-----hello");
            // Iterate through selected divisions
            foreach (var divisionId in selectedDivisionIds)
            {
                var qryMatchBase = _dbContext.Matches.Where(m => m.TournamentId == activeTournament.Id && m.DivisionId == divisionId);

                int matchNumber = 0;
                if (qryMatchBase.Any())
                {
                    matchNumber = qryMatchBase.Max(m => m.MatchNumber);
                }
                List<string> actualGroups;
                if (selectedGroupNames.Contains(string.Empty) == false || selectedGroupNames.Count == 0)
                {
                    var str = qryBase
                    .Where(ttd => ttd.DivisionId == divisionId)
                    .Select(ttd => ttd.GroupName).Distinct().ToQueryString();
                    _logger.LogInformation($"gggg Query : {str}");
                    actualGroups = qryBase
                    .Where(ttd => ttd.DivisionId == divisionId)
                    .Select(ttd => ttd.GroupName).Distinct().ToList();
                }
                else
                {
                    actualGroups = qryBase
                    .Where(ttd => ttd.DivisionId == divisionId)
                    .Where(ttd => selectedGroupNames.Contains(ttd.GroupName))
                    .Select(ttd => ttd.GroupName).Distinct().ToList();
                }
                _logger.LogInformation($"Selected actualGroups: {actualGroups.Count}");
                // Iterate through selected groups
                foreach (var groupName in actualGroups)
                {
                    var qryMatchGroup = qryMatchBase.Where(m => m.GroupName == groupName && m.RoundId == firstRound.Id);
                    // Get teams in the current division and current group
                    var teamIds = qryBaseTTD
                        .Where(ttd => ttd.DivisionId == divisionId && ttd.GroupName == groupName)
                        .OrderBy(ttd => ttd.SeedNumber).Select(ttd => ttd.TeamId).Distinct().ToList();
                    if (teamIds.Count() <= 1)
                    {
                        //TODO error not enought teams
                        continue;
                    }
                    // Generate pairings (round-robin within the group)
                    for (int i = 0; i < teamIds.Count; i++)
                    {
                        for (int j = i + 1; j < teamIds.Count; j++)
                        {
                            var homeTeamId = teamIds[i];
                            var awayTeamId = teamIds[j];

                            if (qryMatchGroup.Any(m => (m.HomeTeamId == homeTeamId && m.AwayTeamId == awayTeamId)
                            || (m.HomeTeamId == awayTeamId && m.AwayTeamId == homeTeamId)))
                            {
                                continue;
                            }

                            // Create match object
                            var match = new Match
                            {
                                Id = Guid.NewGuid(),
                                TournamentId = activeTournament.Id,
                                DivisionId = divisionId,
                                GroupName = groupName,
                                HomeTeamId = homeTeamId,
                                AwayTeamId = awayTeamId,
                                RoundId = firstRound.Id,
                                ScheduledTime = nextMatchTime,
                                MatchNumber = ++matchNumber,
                                CourtLocation = "Unassigned",
                                CreatedBy = "System",
                                UpdatedBy = "System",
                                UpdatedAt = DateTime.UtcNow
                            };
                            matchesToCreate.Add(match);
                            nextMatchTime = nextMatchTime.AddMinutes(10);
                        }
                    }
                }
            }
            _logger.LogInformation($"matchesToCreate: {matchesToCreate.Count}");
            // Save generated matches to the database
            _dbContext.Matches.AddRange(matchesToCreate);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Matches/AutoGenerateNextRound
        public IActionResult AutoGenerateNextRound()
        {
            var viewModel = new AutoGenerateNextRoundViewModel
            {
                Divisions = _dbContext.Divisions.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToList()
            };
            return View(viewModel);
        }

        // POST: Admin/Matches/AutoGenerateNextRound
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoGenerateNextRound(AutoGenerateNextRoundViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Divisions = _dbContext.Divisions.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToList();
                return View(model);
            }

            var selectedDivisionId = model.SelectedDivisionId;
            var teamsToAdvance = model.TeamsToAdvance;
            var selectionMethod = model.SelectionMethod;

            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }

            var currentRoundNumber = await _dbContext.Rounds
                .Where(r => r.Matches.Any(m => m.TournamentId == activeTournament.Id && m.DivisionId == selectedDivisionId))
                .MaxAsync(r => (int?)r.Sequence) ?? 0;
            var nextRoundNumber = currentRoundNumber + 1;

            var nextRound = new Round
            {
                Id = Guid.NewGuid(),
                Name = $"Round {nextRoundNumber}",
                Sequence = nextRoundNumber,
                QualifyingTeams = teamsToAdvance
            };
            _dbContext.Rounds.Add(nextRound);
            await _dbContext.SaveChangesAsync();

            var divisionMatches = await _dbContext.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.TournamentId == activeTournament.Id && m.DivisionId == selectedDivisionId)
                .ToListAsync();

            List<Team> advancingTeams = new List<Team>();

            if (selectionMethod == "TopFromEachGroup")
            {
                var groupedMatches = divisionMatches
                    .GroupBy(m => m.GroupName);

                foreach (var group in groupedMatches)
                {
                    var scoredMatches = group
                        .Where(m => m.HomeTeamScore > -1 && m.AwayTeamScore > -1);

                    if (scoredMatches.Any())
                    {
                        var teamPoints = scoredMatches.SelectMany(m => new[]
                        {
                            new { Team = m.HomeTeam, Points = CalculatePoints(m.HomeTeamScore, m.AwayTeamScore) },
                            new { Team = m.AwayTeam, Points = CalculatePoints(m.AwayTeamScore, m.HomeTeamScore) }
                        })
                        .GroupBy(tp => tp.Team)
                        .Select(g => new { Team = g.Key, TotalPoints = g.Sum(x => x.Points) })
                        .OrderByDescending(tp => tp.TotalPoints)
                        .Take(teamsToAdvance)
                        .ToList();

                        advancingTeams.AddRange(teamPoints.Where(tp => tp.Team != null).Select(tp => tp.Team!));
                    }
                }
            }
            else if (selectionMethod == "TopFromGroupAndNextBest")
            {
                // Implementation for "Top from group and next best"
            }
            else if (selectionMethod == "TopByPoints")
            {
                // Implementation for "Top by points"
            }

            if (advancingTeams.Count >= 2)
            {
                // Create matches for the next round
                for (int i = 0; i < advancingTeams.Count - 1; i += 2)
                {
                    var match = new Match
                    {
                        Id = Guid.NewGuid(),
                        TournamentId = activeTournament.Id,
                        DivisionId = selectedDivisionId,
                        RoundId = nextRound.Id,
                        HomeTeamId = advancingTeams[i].Id,
                        AwayTeamId = advancingTeams[i + 1].Id,
                        ScheduledTime = activeTournament.TournamentDate.AddDays(nextRoundNumber), // Example scheduling
                        CourtLocation = "TBD",
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        GroupName = "TBD"
                    };
                    _dbContext.Matches.Add(match);
                }
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private int CalculatePoints(int homeScore, int awayScore)
        {
            if (homeScore > awayScore)
            {
                return 3;
            }
            else if (homeScore < awayScore)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGroups(Guid[] divisionIds)
        {
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }
            var qry = _dbContext.TournamentTeamDivisions
                .Where(ttd => ttd.TournamentId == activeTournament.Id);
            _logger.LogInformation(divisionIds.Length + " div ids");
            if (divisionIds.Contains(Guid.Empty) == false && divisionIds.Length > 0)
            {
                qry = qry.Where(ttd => divisionIds.Contains(ttd.DivisionId));

            }
            var groups = qry.Select(ttd => new { id = ttd.GroupName, name = ttd.GroupName })
                .Distinct()
                .OrderBy(g => g.name)
                .ToList();
            return Json(groups);
        }

        private bool MatchExists(Guid? id)
        {
            return _dbContext.Matches.Any(e => e.Id == id);
        }
    }
}
