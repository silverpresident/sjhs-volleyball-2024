using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class BulletinController : Controller
    {
        private readonly IBulletinService _bulletinService;

        public BulletinController(IBulletinService bulletinService)
        {
            _bulletinService = bulletinService;
        }

        public async Task<IActionResult> Index()
        {
            var bulletins = await _bulletinService.GetAllBulletinsAsync();
            return View(bulletins.OrderByDescending(a => a.CreatedAt).ToList());
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var bulletin = await _bulletinService.GetBulletinByIdAsync(id);
            if (bulletin == null)
            {
                return NotFound();
            }
            return View(bulletin);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bulletin bulletin)
        {
            if (ModelState.IsValid)
            {
                await _bulletinService.CreateBulletinAsync(bulletin);
                return RedirectToAction(nameof(Index));
            }
            return View(bulletin);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var bulletin = await _bulletinService.GetBulletinByIdAsync(id);
            if (bulletin == null)
            {
                return NotFound();
            }
            return View(bulletin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Bulletin bulletin)
        {
            if (ModelState.IsValid)
            {
                await _bulletinService.UpdateBulletinAsync(bulletin);
                return RedirectToAction(nameof(Index));
            }
            return View(bulletin);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var bulletin = await _bulletinService.GetBulletinByIdAsync(id);
            if (bulletin == null)
            {
                return NotFound();
            }
            return View(bulletin);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _bulletinService.DeleteBulletinAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
