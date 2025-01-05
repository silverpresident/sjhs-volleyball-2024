using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementsApiController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementsApiController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnnouncements()
        {
            var announcements = await _announcementService.GetAllAnnouncementsAsync(includeHidden: true);
            return Ok(announcements);
        }
    }
}
