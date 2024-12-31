using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.Admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class TeamsController : Controller
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        // GET: Admin/Teams
        public async Task<IActionResult> Index()
        {
            var teams = await _teamService.GetTeamsAsync();
            return View(teams);
        }

        // GET: Admin/Teams/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var team = await _teamService.GetTeamAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return View(team);
        }

        // GET: Admin/Teams/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,School,Color,Division,LogoUrl")] Team team)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _teamService.CreateTeamAsync(team);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating team: " + ex.Message);
                }
            }
            return View(team);
        }

        // GET: Admin/Teams/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var team = await _teamService.GetTeamAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return View(team);
        }

        // POST: Admin/Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,School,Color,Division,LogoUrl")] Team team)
        {
            if (id != team.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _teamService.UpdateTeamAsync(team);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    if (await _teamService.GetTeamAsync(id) == null)
                    {
                        return NotFound();
                    }
                    ModelState.AddModelError("", "Error updating team: " + ex.Message);
                }
            }
            return View(team);
        }

        // GET: Admin/Teams/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var team = await _teamService.GetTeamAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return View(team);
        }

        // POST: Admin/Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _teamService.DeleteTeamAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var team = await _teamService.GetTeamAsync(id);
                return View(team);
            }
        }
    }
}
