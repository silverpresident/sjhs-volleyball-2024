using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DivisionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DivisionsController> _logger;

        public DivisionsController(ApplicationDbContext context, ILogger<DivisionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Divisions
        public async Task<IActionResult> Index()
        {
            return View(await _context.Divisions.AsNoTracking().ToListAsync());
        }

        // GET: Admin/Divisions/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var division = await _context.Divisions
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (division == null)
            {
                return NotFound();
            }

            return View(division);
        }

        // GET: Admin/Divisions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Divisions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Division division)
        {
            if (ModelState.IsValid)
            {
                _context.Add(division);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(division);
        }

        // GET: Admin/Divisions/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var division = await _context.Divisions.FindAsync(id);
            if (division == null)
            {
                return NotFound();
            }
            return View(division);
        }

        // POST: Admin/Divisions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name")] Division model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var division = await _context.Divisions.FindAsync(id);
                    if (division == null)
                    {
                        return NotFound();
                    }
                    division.Name = model.Name;
                    division.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DivisionExists(model.Id))
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
            return View(model);
        }

        // GET: Admin/Divisions/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var division = await _context.Divisions
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (division == null)
            {
                return NotFound();
            }

            return View(division);
        }

        // POST: Admin/Divisions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var division = await _context.Divisions.FindAsync(id);
                if (division != null)
                {
                    // Check if division is used in tournaments
                    var hasTournamentDivisions = await _context.TournamentDivisions
                        .AnyAsync(td => td.DivisionId == id);
                    
                    // Check if division has teams
                    var hasTeams = await _context.TournamentTeamDivisions
                        .AnyAsync(ttd => ttd.DivisionId == id);
                    
                    // Check if division has rounds
                    var hasRounds = await _context.TournamentRounds
                        .AnyAsync(tr => tr.DivisionId == id);
                    
                    if (hasTournamentDivisions || hasTeams || hasRounds)
                    {
                        TempData["Error"] = "Cannot delete division. It is being used in tournaments, teams, or rounds.";
                        _logger.LogWarning("Attempted to delete division {DivisionId} which is in use", id);
                        return RedirectToAction(nameof(Index));
                    }
                    
                    _context.Divisions.Remove(division);
                }
                await _context.SaveChangesAsync();
               TempData["Success"] = "Division deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting division {DivisionId}", id);
                TempData["Error"] = "An error occurred while deleting the division.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool DivisionExists(Guid id)
        {
            return _context.Divisions.Any(e => e.Id == id);
        }
    }
}
