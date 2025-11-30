using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BulletinsApiController : ControllerBase
    {
        private readonly IBulletinService _bulletinService;

        public BulletinsApiController(IBulletinService bulletinService)
        {
            _bulletinService = bulletinService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnnouncements()
        {
            var bulletins = await _bulletinService.GetAllBulletinsAsync(includeHidden: true);
            return Ok(bulletins);
        }
    }
}
