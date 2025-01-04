using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Services;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Hubs;

namespace VolleyballRallyManager.App.Controllers
{
    public class MatchesController : Controller
    {
        private readonly IMatchService _matchService;
        private readonly IHubContext<MatchHub> _matchHub;

        public MatchesController(IMatchService matchService, IHubContext<MatchHub> matchHub)
        {
            _matchService = matchService;
            _matchHub = matchHub;
        }

        public async Task<IActionResult> Index(int? divisionId, int? groupId, Guid? roundId)
        {
            var matches = await _matchService.GetMatchesAsync();

   /*         if (divisionId.HasValue)
            {
                matches = matches.Where(m => m.DivisionId == divisionId.Value);
            }

            if (groupId.HasValue)
            {
                matches = matches.Where(m => m.GroupId == groupId.Value);
            }

            if (roundId.HasValue)
            {
                matches = matches.Where(m => m.RoundId == roundId.Value);
            }*/

            return View(matches.ToList());
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var match = await _matchService.GetMatchAsync(id);
            if (match == null)
            {
                return NotFound();
            }
            return View(match);
        }
    }
}
