using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;
using System.Linq;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class MatchesController : Controller
    {

        private readonly IActiveTournamentService _activeTournamentService;
        private readonly ApplicationDbContext _dbContext;

        public MatchesController(IActiveTournamentService activeTournamentService, ApplicationDbContext context)
        {
            _dbContext = context;
            _activeTournamentService = activeTournamentService;
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

            var match = await _dbContext.Matches
                .Include(m => m.AwayTeam)
                .Include(m => m.HomeTeam)
                .Include(m => m.Division)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // GET: Admin/Matches/Create
        public IActionResult Create()
        {
            ViewData["AwayTeamId"] = new SelectList(_dbContext.Teams, "Id", "Name");
            ViewData["HomeTeamId"] = new SelectList(_dbContext.Teams, "Id", "Name");
            return View();
        }

        // POST: Admin/Matches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,HomeTeamId,AwayTeamId,MatchDate,Location,HomeScore,AwayScore,Notes")] Match match)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Add(match);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AwayTeamId"] = new SelectList(_dbContext.Teams, "Id", "Name", match.AwayTeamId);
            ViewData["HomeTeamId"] = new SelectList(_dbContext.Teams, "Id", "Name", match.HomeTeamId);
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
            ViewData["AwayTeamId"] = new SelectList(_dbContext.Teams, "Id", "Name", match.AwayTeamId);
            ViewData["HomeTeamId"] = new SelectList(_dbContext.Teams, "Id", "Name", match.HomeTeamId);
            return View(match);
        }

        // POST: Admin/Matches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,HomeTeamId,AwayTeamId,MatchDate,Location,HomeScore,AwayScore,Notes")] Match match)
        {
            if (id != match.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _dbContext.Update(match);
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchExists(match.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AwayTeamId"] = new SelectList(_dbContext.Teams, "Id", "Name", match.AwayTeamId);
            ViewData["HomeTeamId"] = new SelectList(_dbContext.Teams, "Id", "Name", match.HomeTeamId);
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
        public IActionResult AutoGenerateFirstRound()
        {
            var divisions = _dbContext.Divisions.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
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


            // Remove existing matches if requested
            if (removeExistingMatches)
            {
                var qryRM = _dbContext.Matches
                    .Where(m => m.TournamentId == activeTournament.Id);
                if (selectedDivisionIds.Contains(Guid.Empty) == false && selectedDivisionIds.Count > 0)
                {
                    qryRM = qryRM.Where(ttd => selectedDivisionIds.Contains(ttd.DivisionId));
                }
                if (selectedGroupNames.Contains(string.Empty) == false && selectedGroupNames.Count > 0)
                {
                    qryRM = qryRM.Where(ttd => selectedGroupNames.Contains(ttd.GroupName));
                }
                var existingMatches = qryRM;
                _dbContext.Matches.RemoveRange(existingMatches);
                await _dbContext.SaveChangesAsync();
            }
            var qryBase = _dbContext.TournamentTeamDivisions
                            .Where(ttd => ttd.TournamentId == activeTournament.Id);
            var matchesToCreate = new List<Match>();
            if (selectedDivisionIds.Contains(Guid.Empty) == false || selectedDivisionIds.Count == 0)
            {
                selectedDivisionIds = qryBase
                .Select(ttd => ttd.DivisionId).Distinct().ToList();
            }

            var nextMatchTime = activeTournament.TournamentDate.AddMinutes(15);
            
                            // Get the "First Round" round
                            var firstRound = _dbContext.Rounds.FirstOrDefault(r => r.Name == "Round 1");

                            if (firstRound == null)
                            {
//TODO how to resolve
                            }
            // Iterate through selected divisions
            foreach (var divisionId in selectedDivisionIds)
            {
                int matchNumber = _dbContext.Matches.Where(m => m.TournamentId == activeTournament.Id && m.DivisionId == divisionId).Max(m => m.MatchNumber);
                List<string> actualGroups;
                if (selectedGroupNames.Contains(string.Empty) == false && selectedGroupNames.Count == 0)
                {
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
                // Iterate through selected groups
                foreach (var groupName in actualGroups)
                {
                    // Get teams in the current division and current group
                    var teamIds = qryBaseTTD
                        .Where(ttd => ttd.DivisionId == divisionId && ttd.GroupName == groupName)
                        .OrderBy(ttd => ttd.SeedNumber).Select(ttd => ttd.TeamId).Distinct().ToList();

                    // Generate pairings (round-robin within the group)
                    for (int i = 0; i < teamIds.Count; i++)
                    {
                        for (int j = i + 1; j < teamIds.Count; j++)
                        {
                            var homeTeamId = teamIds[i];
                            var awayTeamId = teamIds[j];

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
            // Save generated matches to the database
            _dbContext.Matches.AddRange(matchesToCreate);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetGroups(Guid[] divisionIds)
        {
            var groups = _dbContext.TournamentTeamDivisions
                .Where(ttd => divisionIds.Contains(ttd.DivisionId))
                .Select(ttd => new { id = ttd.GroupName, name = ttd.GroupName })
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
