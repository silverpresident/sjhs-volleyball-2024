using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class AnnouncementsController : Controller
    {
        private readonly IAnnouncementService _announcementService;
        private readonly ILogger<AnnouncementsController> _logger;

        public AnnouncementsController(
            IAnnouncementService announcementService,
            ILogger<AnnouncementsController> logger)
        {
            _announcementService = announcementService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var announcements = await _announcementService.GetAllAnnouncementsAsync(includeHidden: true);
                return View(announcements.OrderBy(a => a.SequencingNumber).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading announcements index");
                TempData["Error"] = "Failed to load announcements.";
                return View(new List<Announcement>());
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
                if (announcement == null)
                {
                    return NotFound();
                }

                var history = await _announcementService.GetHistoryForAnnouncementAsync(id);
                ViewBag.History = history;

                return View(announcement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading announcement details for {Id}", id);
                return NotFound();
            }
        }

        public IActionResult Create()
        {
            return View(new Announcement { RemainingRepeatCount = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement announcement)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //TODO CreatedBY
                    await _announcementService.CreateAnnouncementAsync(announcement);
                    TempData["Success"] = "Announcement created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(announcement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating announcement");
                TempData["Error"] = "Failed to create announcement.";
                return View(announcement);
            }
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
                if (announcement == null)
                {
                    return NotFound();
                }
                return View(announcement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading announcement for edit {Id}", id);
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Announcement announcement)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //TODO updated by
                    await _announcementService.UpdateAnnouncementAsync(announcement);
                    TempData["Success"] = "Announcement updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(announcement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating announcement {Id}", announcement.Id);
                TempData["Error"] = "Failed to update announcement.";
                return View(announcement);
            }
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
                if (announcement == null)
                {
                    return NotFound();
                }
                return View(announcement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading announcement for delete {Id}", id);
                return NotFound();
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _announcementService.DeleteAnnouncementAsync(id);
                TempData["Success"] = "Announcement deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting announcement {Id}", id);
                TempData["Error"] = "Failed to delete announcement.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHide(Guid id)
        {
            try
            {
                var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
                if (announcement == null)
                {
                    return NotFound();
                }

                if (announcement.IsHidden)
                {
                    await _announcementService.UnhideAnnouncementAsync(id);
                    TempData["Success"] = "Announcement unhidden successfully.";
                }
                else
                {
                    await _announcementService.HideAnnouncementAsync(id);
                    TempData["Success"] = "Announcement hidden successfully.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling hide for announcement {Id}", id);
                TempData["Error"] = "Failed to toggle announcement visibility.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reannounce(Guid id)
        {
            try
            {
                await _announcementService.ReannounceAsync(id);
                TempData["Success"] = "Announcement requeued successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reannouncing announcement {Id}", id);
                TempData["Error"] = "Failed to reannounce.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
