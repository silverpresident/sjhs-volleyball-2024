using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TournamentController : Controller
    {
        private readonly ITournamentService _tournamentService;
        private readonly ApplicationDbContext _context;

        public TournamentController(ITournamentService tournamentService, ApplicationDbContext context)
        {
            _tournamentService = tournamentService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tournaments = await _tournamentService.GetAllTournamentsAsync();
            return View(tournaments);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                await _tournamentService.CreateTournamentAsync(tournament);
                return RedirectToAction(nameof(Index));
            }
            return View(tournament);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var tournament = await _tournamentService.GetTournamentByIdAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }

            TournamentDetailsViewModel viewModel = await _tournamentService.GetTournamentDetailsAsync(tournament.Id);
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var tournament = await _tournamentService.GetTournamentByIdAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }
            return View(tournament);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                await _tournamentService.UpdateTournamentAsync(tournament);
                return RedirectToAction(nameof(Index));
            }
            return View(tournament);
        }

        public async Task<IActionResult> SetActive(Guid id)
        {
            await _tournamentService.SetActiveTournamentAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
