using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.App.Areas.Admin.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DeveloperController : Controller
    {
        private readonly ITeamGenerationService _teamGenerationService;
        private readonly IActiveTournamentService _activeTournamentService;
        private readonly IMatchService _matchService;
        private readonly ITournamentRoundService _tournamentRoundService;
        private readonly ILogger<DeveloperController> _logger;

        public DeveloperController(
            ITeamGenerationService teamGenerationService,
            IActiveTournamentService activeTournamentService,
            IMatchService matchService,
            ITournamentRoundService tournamentRoundService,
            ILogger<DeveloperController> logger)
        {
            _teamGenerationService = teamGenerationService;
            _activeTournamentService = activeTournamentService;
            _matchService = matchService;
            _tournamentRoundService = tournamentRoundService;
            _logger = logger;
        }

        // GET: Admin/Developer
        public async Task<IActionResult> Index()
        {
#if !DEBUG
            return NotFound();
#endif
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                TempData["ErrorMessage"] = "No active tournament found. Please set an active tournament first.";
            }

            return View();
        }

        // GET: Admin/Developer/GenerateTeams
        public async Task<IActionResult> GenerateTeams()
        {
#if !DEBUG
            return NotFound();
#endif
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                TempData["ErrorMessage"] = "No active tournament found. Please set an active tournament first.";
                return RedirectToAction(nameof(Index));
            }

            var model = new GenerateTeamsViewModel
            {
                NumberOfTeams = 10
            };

            return View(model);
        }

        // POST: Admin/Developer/GenerateTeams
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateTeams(GenerateTeamsViewModel model)
        {
#if !DEBUG
            return NotFound();
#endif
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                TempData["ErrorMessage"] = "No active tournament found. Please set an active tournament first.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _logger.LogInformation("Generating {Count} random teams for active tournament", model.NumberOfTeams);
                
                var teams = await _teamGenerationService.GenerateRandomTeamsAsync(model.NumberOfTeams);
                
                TempData["SuccessMessage"] = $"Successfully generated {teams.Count()} teams!";
                _logger.LogInformation("Successfully generated {Count} teams", teams.Count());
                
                return RedirectToAction("Index", "Teams");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating teams");
                ModelState.AddModelError("", $"Error generating teams: {ex.Message}");
                return View(model);
            }
        }

        // GET: Admin/Developer/DeleteAllMatches
        public async Task<IActionResult> DeleteAllMatches()
        {
#if !DEBUG
            return NotFound();
#endif
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                TempData["ErrorMessage"] = "No active tournament found. Please set an active tournament first.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TournamentName = activeTournament.Name;
            return View();
        }

        // POST: Admin/Developer/DeleteAllMatches
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllMatchesConfirmed()
        {
#if !DEBUG
            return NotFound();
#endif
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                TempData["ErrorMessage"] = "No active tournament found. Please set an active tournament first.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Deleting all matches for tournament {TournamentId}", activeTournament.Id);
                
                var deletedCount = await _matchService.DeleteAllMatchesByTournamentAsync(activeTournament.Id);
                
                TempData["SuccessMessage"] = $"Successfully deleted {deletedCount} match(es) from the active tournament!";
                _logger.LogInformation("Successfully deleted {Count} matches", deletedCount);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting matches");
                TempData["ErrorMessage"] = $"Error deleting matches: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Developer/DeleteAllRounds
        public async Task<IActionResult> DeleteAllRounds()
        {
#if !DEBUG
            return NotFound();
#endif
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                TempData["ErrorMessage"] = "No active tournament found. Please set an active tournament first.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TournamentName = activeTournament.Name;
            return View();
        }

        // POST: Admin/Developer/DeleteAllRounds
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllRoundsConfirmed()
        {
#if !DEBUG
            return NotFound();
#endif
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                TempData["ErrorMessage"] = "No active tournament found. Please set an active tournament first.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Deleting all rounds for tournament {TournamentId}", activeTournament.Id);
                
                var deletedCount = await _tournamentRoundService.DeleteAllRoundsByTournamentAsync(activeTournament.Id);
                
                TempData["SuccessMessage"] = $"Successfully deleted {deletedCount} round(s) from the active tournament!";
                _logger.LogInformation("Successfully deleted {Count} rounds", deletedCount);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rounds");
                TempData["ErrorMessage"] = $"Error deleting rounds: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Developer/GenerateMatchResults
        public async Task<IActionResult> GenerateMatchResults()
        {
#if !DEBUG
            return NotFound();
#endif
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                TempData["ErrorMessage"] = "No active tournament found. Please set an active tournament first.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TournamentName = activeTournament.Name;
            return View();
        }

        // POST: Admin/Developer/GenerateMatchResults
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateMatchResultsConfirmed()
        {
#if !DEBUG
            return NotFound();
#endif
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                TempData["ErrorMessage"] = "No active tournament found. Please set an active tournament first.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Generating match results for tournament {TournamentId}", activeTournament.Id);

                // Get all unfinished matches in the active tournament
                var allMatches = await _matchService.GetMatchesAsync();
                var unfinishedMatches = allMatches
                    .Where(m => m.TournamentId == activeTournament.Id && !m.IsFinished)
                    .ToList();

                if (unfinishedMatches.Count == 0)
                {
                    TempData["SuccessMessage"] = "No unfinished matches found in the active tournament.";
                    return RedirectToAction(nameof(Index));
                }

                var random = new Random();
                var systemUser = "System";
                int matchesProcessed = 0;

                foreach (var match in unfinishedMatches)
                {
                    // Randomly decide if match will have 2 or 3 sets (2 sets = 2-0, 3 sets = 2-1)
                    int numberOfSets = random.Next(2, 4); // 2 or 3 sets
                    int homeWins = 0;
                    int awayWins = 0;

                    // Start the match if not started
                    if (match.CurrentSetNumber == 0)
                    {
                        await _matchService.StartMatchAsync(match.Id, systemUser);
                    }

                    // Generate sets
                    for (int setNumber = 1; setNumber <= numberOfSets; setNumber++)
                    {
                        // Determine winner for this set
                        bool homeTeamWins;
                        if (setNumber < numberOfSets)
                        {
                            // For first sets, random winner
                            homeTeamWins = random.Next(2) == 0;
                        }
                        else
                        {
                            // Last set: whoever needs to reach 2 wins
                            homeTeamWins = homeWins < 2;
                        }

                        if (homeTeamWins)
                        {
                            homeWins++;
                        }
                        else
                        {
                            awayWins++;
                        }

                        // Generate realistic volleyball scores
                        int winningScore = 25; // Standard winning score
                        int losingScore = random.Next(15, 25); // Losing team gets 15-24 points
                        
                        // If it's a close game, winning score might be higher (deuce)
                        if (losingScore >= 23)
                        {
                            winningScore = losingScore + 2; // Must win by 2
                        }

                        int homeScore = homeTeamWins ? winningScore : losingScore;
                        int awayScore = homeTeamWins ? losingScore : winningScore;

                        // Create/update the set with scores
                        var matchSet = await _matchService.GetOrCreateMatchSetAsync(match.Id, setNumber, systemUser);
                        await _matchService.UpdateSetScoreAsync(match.Id, setNumber, homeScore, awayScore, systemUser);
                        
                        // Finalize the set
                        await _matchService.FinishSetAsync(match.Id, setNumber, systemUser);
                    }

                    // Finalize the match
                    await _matchService.FinishMatchAsync(match.Id, systemUser);
                    matchesProcessed++;
                    
                    _logger.LogInformation("Generated results for match {MatchId}: {Home}-{Away} in {Sets} sets", 
                        match.Id, homeWins, awayWins, numberOfSets);
                }

                TempData["SuccessMessage"] = $"Successfully generated results for {matchesProcessed} match(es)!";
                _logger.LogInformation("Successfully generated results for {Count} matches", matchesProcessed);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating match results");
                TempData["ErrorMessage"] = $"Error generating match results: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
