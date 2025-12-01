using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.App.Areas.Admin.Models;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Services;
using Microsoft.Extensions.Logging;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TournamentTeamsController : Controller
    {
        private readonly IActiveTournamentService _activeTournamentService;
        private readonly ITournamentService _tournamentService;
        private readonly ILogger<TournamentTeamsController> _logger;

        public TournamentTeamsController(
            IActiveTournamentService activeTournamentService, 
            ITournamentService tournamentService,
            ILogger<TournamentTeamsController> logger)
        {
            _activeTournamentService = activeTournamentService;
            _tournamentService = tournamentService;
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
            //model = model.OrderBy(ttd => ttd.GroupName);
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
        public async Task<IActionResult> Create([Bind("TeamId,DivisionId,TournamentId,GroupName,SeedNumber")] TournamentTeamAddViewModel tournamentTeamDivision)
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
                await _activeTournamentService.AddTeamAsync(tournamentTeamDivision.TeamId, tournamentTeamDivision.DivisionId, tournamentTeamDivision.GroupName ?? "", tournamentTeamDivision.SeedNumber);
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
                SeedNumber = tournamentTeamDivision.SeedNumber
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("TeamId,DivisionId,TournamentId,GroupName,SeedNumber")] TournamentTeamAddViewModel tournamentTeamDivision)
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
                    await _activeTournamentService.SetTeamAsync(tournamentTeamDivision.TeamId, tournamentTeamDivision.DivisionId, tournamentTeamDivision.GroupName ?? "", tournamentTeamDivision.SeedNumber);
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
                SeedNumber = tournamentTeamDivision.SeedNumber
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
    }
}
