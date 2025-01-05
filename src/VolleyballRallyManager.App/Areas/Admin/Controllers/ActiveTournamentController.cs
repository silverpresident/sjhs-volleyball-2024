using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActiveTournamentController : Controller
    {
        private readonly IActiveTournamentService _activeTournamentService;

        public ActiveTournamentController(IActiveTournamentService activeTournamentService)
        {
            _activeTournamentService = activeTournamentService;
        }

        public async Task<IActionResult> Index()
        {
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound();
            }

            var divisions = await _activeTournamentService.GetTournamentDivisionsAsync();
            var teamsByDivision = new Dictionary<TournamentDivision, IEnumerable<TournamentTeamDivision>>();

            foreach (var division in divisions)
            {
                var teams = await _activeTournamentService.GetTournamentTeamsAsync(division.DivisionId);
                teamsByDivision.Add(division, teams);
            }

            var viewModel = new ActiveTournamentViewModel
            {
                ActiveTournament = activeTournament,
                Divisions = divisions,
                TeamsByDivision = teamsByDivision
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SelectDivisions()
        {
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                return NotFound();
            }

            ModelState.AddModelError("A","WhyZZ");
            var availableDivisions = await _activeTournamentService.GetAvailableDivisionsAsync();
            var viewModel = new SelectDivisionsViewModel
            {
                Tournament = activeTournament,
                AvailableDivisions = availableDivisions,
                SelectedDivisionIds = activeTournament.TournamentDivisions.Select(td => td.DivisionId).ToList() ?? new List<Guid>()
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SelectDivisions([Bind("SelectedDivisionIds")]SelectDivisionsViewModel model)
        {
            ModelState.AddModelError("A","Why");
            ModelState.AddModelError("A",string.Join(",", model.SelectedDivisionIds.Select(m => m.ToString())));
            if (ModelState.IsValid)
            {
                var divisionIds = model.SelectedDivisionIds;
                await _activeTournamentService.UpdateTournamentDivisionsAsync(divisionIds);
                return RedirectToAction(nameof(Index));
            }
            model.AvailableDivisions = await _activeTournamentService.GetAvailableDivisionsAsync();
            return View(model);
        }
    }
}
