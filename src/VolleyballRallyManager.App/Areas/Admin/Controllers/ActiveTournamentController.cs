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
        private readonly ILogger<ActiveTournamentController> _logger;

        public ActiveTournamentController(IActiveTournamentService activeTournamentService, ILogger<ActiveTournamentController> logger)
        {
            _activeTournamentService = activeTournamentService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Executing Index action");
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                _logger.LogWarning("Active tournament not found");
                return NotFound();
            }

            var viewModel = new ActiveTournamentViewModel
            {
                ActiveTournament = activeTournament,
                Divisions = new List<Division>()
            };
            var tournamentDivisions = await _activeTournamentService.GetTournamentDivisionsAsync();
            var teamsByDivision = new Dictionary<TournamentDivision, IEnumerable<TournamentTeamDivision>>();

            foreach (var division in tournamentDivisions)
            {
                var teams = await _activeTournamentService.GetTournamentTeamsAsync(division.DivisionId);
                teamsByDivision.Add(division, teams);
                viewModel.Divisions.Add(division.Division);
            }
            viewModel.TournamentDivisions = tournamentDivisions;
            viewModel.TeamsByDivision = teamsByDivision;


            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SelectDivisions()
        {
            _logger.LogInformation("Executing SelectDivisions action");
            var activeTournament = await _activeTournamentService.GetActiveTournamentAsync();
            if (activeTournament == null)
            {
                _logger.LogWarning("Active tournament not found");
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
            _logger.LogInformation("Executing SelectDivisions post!!");
           // ModelState.AddModelError("A0","Why");
           // ModelState.AddModelError("A1",ModelState.IsValid.ToString());
           // ModelState.AddModelError("A2",string.Join(",", model.SelectedDivisionIds.Select(m => m.ToString())));
            if (ModelState.IsValid)
            {
            ModelState.AddModelError("A","IsValid");
                var divisionIds = model.SelectedDivisionIds;
                await _activeTournamentService.UpdateTournamentDivisionsAsync(divisionIds);
                return RedirectToAction(nameof(Index));
            }
            model.AvailableDivisions = await _activeTournamentService.GetAvailableDivisionsAsync();
            return View(model);
        }
    }
}
