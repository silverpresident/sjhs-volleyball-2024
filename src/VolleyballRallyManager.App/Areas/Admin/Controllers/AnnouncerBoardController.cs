using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class AnnouncerBoardController : Controller
    {
        private readonly IAnnouncementService _announcementService;
        private readonly ILogger<AnnouncerBoardController> _logger;

        public AnnouncerBoardController(
            IAnnouncementService announcementService,
            ILogger<AnnouncerBoardController> logger)
        {
            _announcementService = announcementService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var announcements = await _announcementService.GetQueuedAnnouncementsAsync();
                return View(announcements.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading announcer board");
                TempData["Error"] = "Failed to load announcer board.";
                return View(new List<VolleyballRallyManager.Lib.Models.Announcement>());
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Call(Guid id)
        {
            try
            {
                var announcement = await _announcementService.CallAnnouncementAsync(id);
                
                if (announcement.IsHidden)
                {
                    TempData["Success"] = $"Announcement '{announcement.Title}' called and completed.";
                }
                else
                {
                    TempData["Success"] = $"Announcement '{announcement.Title}' called. {announcement.RemainingRepeatCount} repeat(s) remaining.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling announcement {Id}", id);
                TempData["Error"] = "Failed to call announcement.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Defer(Guid id)
        {
            try
            {
                await _announcementService.DeferAnnouncementAsync(id);
                TempData["Success"] = "Announcement deferred to end of queue.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deferring announcement {Id}", id);
                TempData["Error"] = "Failed to defer announcement.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
