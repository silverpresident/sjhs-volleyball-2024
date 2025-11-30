using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Controllers
{
    public class BulletinController : Controller
    {
        private readonly IBulletinService _bulletinService;

        public BulletinController(IBulletinService bulletinService)
        {
            _bulletinService = bulletinService;
        }

        public async Task<IActionResult> Index()
        {
            var bulletins = await _bulletinService.GetAllBulletinsAsync(includeHidden: true);
            return View(bulletins);
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
    }
}
