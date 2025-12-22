using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class ActiveTournamentController : Controller
    {
        private readonly IActiveTournamentService _activeTournamentService;
        private readonly ITournamentService _tournamentService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ActiveTournamentController> _logger;

        public ActiveTournamentController(IActiveTournamentService activeTournamentService, ApplicationDbContext context, ILogger<ActiveTournamentController> logger, ITournamentService tournamentService)
        {
            _activeTournamentService = activeTournamentService;
            _context = context;
            _logger = logger;
            _tournamentService = tournamentService;
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

            var tournamentId = activeTournament.Id;

            TournamentDetailsViewModel viewModel = await _tournamentService.GetTournamentDetailsAsync(tournamentId);
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
        public async Task<IActionResult> SelectDivisions([Bind("SelectedDivisionIds")] SelectDivisionsViewModel model)
        {
            _logger.LogInformation("Executing SelectDivisions post!!");
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
