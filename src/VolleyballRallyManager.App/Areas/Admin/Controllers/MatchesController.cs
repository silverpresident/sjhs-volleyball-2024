using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class MatchesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MatchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Matches
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Matches.Include(m => m.AwayTeam).Include(m => m.HomeTeam);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/Matches/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.AwayTeam)
                .Include(m => m.HomeTeam)
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
            ViewData["AwayTeamId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Teams, "Id", "Name");
            ViewData["HomeTeamId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Teams, "Id", "Name");
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
                _context.Add(match);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AwayTeamId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Teams, "Id", "Name", match.AwayTeamId);
            ViewData["HomeTeamId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Teams, "Id", "Name", match.HomeTeamId);
            return View(match);
        }

        // GET: Admin/Matches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matches.FindAsync(id);
            if (match == null)
            {
                return NotFound();
            }
            ViewData["AwayTeamId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Teams, "Id", "Name", match.AwayTeamId);
            ViewData["HomeTeamId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Teams, "Id", "Name", match.HomeTeamId);
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
                    _context.Update(match);
                    await _context.SaveChangesAsync();
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
            ViewData["AwayTeamId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Teams, "Id", "Name", match.AwayTeamId);
            ViewData["HomeTeamId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Teams, "Id", "Name", match.HomeTeamId);
            return View(match);
        }

        // GET: Admin/Matches/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var match = await _context.Matches
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match != null)
            {
                _context.Matches.Remove(match);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MatchExists(Guid? id)
        {
            return _context.Matches.Any(e => e.Id == id);
        }
    }
}
