using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Data;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoundsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public RoundsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var rounds = _dbContext.Rounds.ToList();
            return View(rounds);
        }
    }
}
