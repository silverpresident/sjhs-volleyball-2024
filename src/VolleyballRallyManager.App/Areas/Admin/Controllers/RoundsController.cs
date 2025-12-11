using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.App.Areas.Admin.Models;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class RoundsController : Controller
    {
        private readonly IRoundService _roundService;
        private readonly ILogger<RoundsController> _logger;

        public RoundsController(IRoundService roundService, ILogger<RoundsController> logger)
        {
            _roundService = roundService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var rounds = await _roundService.GetRoundsWithMatchesAsync();
                
                var roundViewModels = new List<RoundViewModel>();
                int totalMatches = 0;
                int totalCompleted = 0;
                
                foreach (var round in rounds)
                {
                    var totalMatchCount = round.Matches.Count;
                    var completedMatchCount = round.Matches.Count(m => m.IsFinished);
                    var pendingMatchCount = totalMatchCount - completedMatchCount;
                    
                    totalMatches += totalMatchCount;
                    totalCompleted += completedMatchCount;
                    
                    var completionPercentage = totalMatchCount > 0 
                        ? (double)completedMatchCount / totalMatchCount * 100 
                        : 0;
                    
                    roundViewModels.Add(new RoundViewModel
                    {
                        Id = round.Id,
                        Name = round.Name,
                        Sequence = round.Sequence,
                        RecommendedQualifyingTeamsCount = round.RecommendedQualifyingTeamsCount,
                        RecommendedMatchGenerationStrategy = round.RecommendedMatchGenerationStrategy,
                        RecommendedTeamSelectionStrategy = round.RecommendedTeamSelectionStrategy,
                        IsPlayoff = round.IsPlayoff,
                        TotalMatches = totalMatchCount,
                        CompletedMatches = completedMatchCount,
                        PendingMatches = pendingMatchCount,
                        CompletionPercentage = completionPercentage,
                        IsComplete = totalMatchCount > 0 && completedMatchCount == totalMatchCount
                    });
                }
                
                var viewModel = new RoundsIndexViewModel
                {
                    Rounds = roundViewModels.OrderBy(r => r.Sequence),
                    TotalRounds = roundViewModels.Count,
                    TotalMatches = totalMatches,
                    TotalCompletedMatches = totalCompleted,
                    TotalPendingMatches = totalMatches - totalCompleted
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading rounds index page");
                return View("Error");
            }
        }

        // GET: Admin/Rounds/Create
        public IActionResult Create()
        {
            var model = new CreateEditRoundViewModel
            {
                Sequence = 1,
                RecommendedMatchGenerationStrategy = MatchGenerationStrategy.RoundRobin,
                RecommendedTeamSelectionStrategy = TeamSelectionStrategy.TopByPoints
            };
            return View(model);
        }

        // POST: Admin/Rounds/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEditRoundViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userName = User.Identity?.Name ?? "admin";

                var round = new Round
                {
                    Name = model.Name,
                    Sequence = model.Sequence,
                    RecommendedQualifyingTeamsCount = model.RecommendedQualifyingTeamsCount,
                    RecommendedMatchGenerationStrategy = model.RecommendedMatchGenerationStrategy,
                    RecommendedTeamSelectionStrategy = model.RecommendedTeamSelectionStrategy,
                    IsPlayoff = model.IsPlayoff,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _roundService.CreateRoundAsync(round);
                _logger.LogInformation("Created round '{RoundName}' by user {UserName}", round.Name, userName);
                
                TempData["SuccessMessage"] = $"Round '{round.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating round");
                ModelState.AddModelError("", "Error creating round. Please try again.");
                return View(model);
            }
        }

        // GET: Admin/Rounds/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var round = await _roundService.GetRoundByIdAsync(id);
                if (round == null)
                {
                    _logger.LogWarning("Round {RoundId} not found for editing", id);
                    return NotFound();
                }

                var model = new CreateEditRoundViewModel
                {
                    Id = round.Id,
                    Name = round.Name,
                    Sequence = round.Sequence,
                    RecommendedQualifyingTeamsCount = round.RecommendedQualifyingTeamsCount,
                    RecommendedMatchGenerationStrategy = round.RecommendedMatchGenerationStrategy,
                    RecommendedTeamSelectionStrategy = round.RecommendedTeamSelectionStrategy,
                    IsPlayoff = round.IsPlayoff
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for round {RoundId}", id);
                TempData["ErrorMessage"] = "Error loading round for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Rounds/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CreateEditRoundViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (!model.Id.HasValue)
                {
                    return BadRequest("Round ID is required for editing.");
                }

                var round = await _roundService.GetRoundByIdAsync(model.Id.Value);
                if (round == null)
                {
                    _logger.LogWarning("Round {RoundId} not found for update", model.Id.Value);
                    return NotFound();
                }

                var userName = User.Identity?.Name ?? "admin";

                round.Name = model.Name;
                round.Sequence = model.Sequence;
                round.RecommendedQualifyingTeamsCount = model.RecommendedQualifyingTeamsCount;
                round.RecommendedMatchGenerationStrategy = model.RecommendedMatchGenerationStrategy;
                round.RecommendedTeamSelectionStrategy = model.RecommendedTeamSelectionStrategy;
                round.IsPlayoff = model.IsPlayoff;
                round.UpdatedBy = userName;
                round.UpdatedAt = DateTime.Now;

                await _roundService.UpdateRoundAsync(round);
                _logger.LogInformation("Updated round '{RoundName}' by user {UserName}", round.Name, userName);
                
                TempData["SuccessMessage"] = $"Round '{round.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating round {RoundId}", model.Id);
                ModelState.AddModelError("", "Error updating round. Please try again.");
                return View(model);
            }
        }

        // POST: Admin/Rounds/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var round = await _roundService.GetRoundByIdAsync(id);
                if (round == null)
                {
                    _logger.LogWarning("Round {RoundId} not found for deletion", id);
                    return NotFound();
                }

                var userName = User.Identity?.Name ?? "admin";
                
                await _roundService.DeleteRoundAsync(id);
                _logger.LogInformation("Deleted round '{RoundName}' by user {UserName}", round.Name, userName);
                
                TempData["SuccessMessage"] = $"Round '{round.Name}' deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting round {RoundId}", id);
                TempData["ErrorMessage"] = "Error deleting round. It may be in use by existing matches or tournament rounds.";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
