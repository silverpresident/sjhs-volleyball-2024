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
    }
}
