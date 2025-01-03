using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Services;
using System.Threading.Tasks;

namespace VolleyballRallyManager.App.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementsController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        public async Task<IActionResult> Index()
        {
            var announcements = await _announcementService.GetAnnouncementsAsync(includeHidden: true);
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
    }
}
