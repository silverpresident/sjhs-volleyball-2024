using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Services;
using VolleyballRallyManager.Lib.Models;
using System;
using System.Threading.Tasks;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AnnouncementsController : Controller
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementsController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        public async Task<IActionResult> Index()
        {
            var announcements = await _announcementService.GetAnnouncementsAsync();
            return View(announcements);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                await _announcementService.CreateAnnouncementAsync(announcement);
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                await _announcementService.UpdateAnnouncementAsync(announcement);
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _announcementService.DeleteAnnouncementAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
