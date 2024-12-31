using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Web.Areas.Admin.Controllers
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
            return View(await _context.Updates.ToListAsync());
        }

        // GET: Admin/Updates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var update = await _context.Updates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (update == null)
            {
                return NotFound();
            }

            return View(update);
        }

        // GET: Admin/Updates/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Updates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,UpdateType,CreatedDate")] Update update)
        {
            if (ModelState.IsValid)
            {
                _context.Add(update);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(update);
        }

        // GET: Admin/Updates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var update = await _context.Updates.FindAsync(id);
            if (update == null)
            {
                return NotFound();
            }
            return View(update);
        }

        // POST: Admin/Updates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,UpdateType,CreatedDate")] Update update)
        {
            if (id != update.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(update);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UpdateExists(update.Id))
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
            return View(update);
        }

        // GET: Admin/Updates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var update = await _context.Updates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (update == null)
            {
                return NotFound();
            }

            return View(update);
        }

        // POST: Admin/Updates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var update = await _context.Updates.FindAsync(id);
            if (update != null)
            {
                _context.Updates.Remove(update);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UpdateExists(int id)
        {
            return _context.Updates.Any(e => e.Id == id);
        }
    }
}
