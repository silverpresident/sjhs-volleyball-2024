using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.App.Areas.Admin.Models;
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
                        QualifyingTeams = round.QualifyingTeams,
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
    }
}
