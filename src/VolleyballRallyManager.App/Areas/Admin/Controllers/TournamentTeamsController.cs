using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Services;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.App.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TournamentTeamsController : Controller
    {
        private readonly IActiveTournamentService _activeTournamentService;
        private readonly ITournamentService _tournamentService;

        public TournamentTeamsController(IActiveTournamentService activeTournamentService, ITournamentService tournamentService)
        {
            _activeTournamentService = activeTournamentService;
            _tournamentService = tournamentService;
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
            return View(model);
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
        public async Task<IActionResult> Edit(Guid id,[Bind("TeamId,DivisionId,TournamentId,GroupName,SeedNumber")] TournamentTeamAddViewModel tournamentTeamDivision)
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
