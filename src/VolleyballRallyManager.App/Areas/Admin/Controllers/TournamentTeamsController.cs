using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Services;
using VolleyballRallyManager.Lib.Models;
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
            var model = await _activeTournamentService.GetTournamentTeamsAsync(activeTournament.Id);
            return View(model);
        }

        public IActionResult Create(Guid divisionId)
        {
            ViewBag.DivisionId = divisionId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid divisionId, [Bind("TeamId")] TournamentTeamDivision tournamentTeamDivision)
        {
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }

            var division = activeTournament.TournamentDivisions.FirstOrDefault(d => d.DivisionId == divisionId);
            if (division == null)
            {
                return NotFound("Division not found in the active tournament.");
            }

            if (ModelState.IsValid)
            {
                await _activeTournamentService.AddTeamAsync(tournamentTeamDivision.TeamId, tournamentTeamDivision.DivisionId, tournamentTeamDivision.GroupName, tournamentTeamDivision.SeedNumber);
                return RedirectToAction(nameof(Index), new { divisionId });
            }
            return View(tournamentTeamDivision);
        }

        public async Task<IActionResult> Edit(Guid id, Guid divisionId)
        {
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }

            var division = activeTournament.TournamentDivisions.FirstOrDefault(d => d.DivisionId == divisionId);
            if (division == null)
            {
                return NotFound("Division not found in the active tournament.");
            }

            var tournamentTeamDivision = await _activeTournamentService.GetTeamAsync(id);
            if (tournamentTeamDivision == null)
            {
                return NotFound();
            }
            return View(tournamentTeamDivision);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Guid divisionId, [Bind("Id,TeamId")] TournamentTeamDivision tournamentTeamDivision)
        {
            if (id != tournamentTeamDivision.Id)
            {
                return NotFound();
            }

            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound("No active tournament found.");
            }

            var division = activeTournament.TournamentDivisions.FirstOrDefault(d => d.DivisionId == divisionId);
            if (division == null)
            {
                return NotFound("Division not found in the active tournament.");
            }

            if (ModelState.IsValid)
            {
                var existingTeamDivision = await _activeTournamentService.GetTeamAsync(tournamentTeamDivision.TeamId);
                if (existingTeamDivision != null)
                {
                    await _activeTournamentService.SetTeamAsync(tournamentTeamDivision.TeamId, tournamentTeamDivision.DivisionId, tournamentTeamDivision.GroupName, tournamentTeamDivision.SeedNumber);
                    return RedirectToAction(nameof(Index), new { divisionId });
                }
                return NotFound();
            }
            return View(tournamentTeamDivision);
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
