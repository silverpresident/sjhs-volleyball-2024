using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class UpdatesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UpdatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Updates
        public async Task<IActionResult> Index()
        {
            return View(await _context.MatchUpdates.ToListAsync());
        }

        // GET: Admin/Updates/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var matchUpdate = await _context.MatchUpdates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (matchUpdate == null)
            {
                return NotFound();
            }

            return View(matchUpdate);
        }
        

        // GET: Admin/Updates/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var matchUpdate = await _context.MatchUpdates.FindAsync(id);
            if (matchUpdate == null)
            {
                return NotFound();
            }
            return View(matchUpdate);
        }

        // POST: Admin/Updates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Title,Content,UpdateType,CreatedDate")] MatchUpdate matchUpdate)
        {
            if (id != matchUpdate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(matchUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UpdateExists(matchUpdate.Id))
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
            return View(matchUpdate);
        }

        // GET: Admin/Updates/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var matchUpdate = await _context.MatchUpdates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (matchUpdate == null)
            {
                return NotFound();
            }

            return View(matchUpdate);
        }

        // POST: Admin/Updates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var matchUpdate = await _context.MatchUpdates.FindAsync(id);
            if (matchUpdate != null)
            {
                _context.MatchUpdates.Remove(matchUpdate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UpdateExists(Guid id)
        {
            return _context.MatchUpdates.Any(e => e.Id == id);
        }
    }
}
