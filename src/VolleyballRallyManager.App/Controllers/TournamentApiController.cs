using Microsoft.AspNetCore.Mvc;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TournamentApiController : ControllerBase
    {
        private readonly IActiveTournamentService _activeTournamentService;
        private readonly ILogger<TournamentApiController> _logger;

        public TournamentApiController(IActiveTournamentService activeTournamentService, ILogger<TournamentApiController> logger)
        {
            _activeTournamentService = activeTournamentService;
            _logger = logger;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveTournament()
        {
            try
            {
                var tournament = await _activeTournamentService.GetActiveTournamentAsync();
                if (tournament == null)
                {
                    return NotFound();
                }
                return Ok(tournament);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active tournament");
                return StatusCode(500, "An error occurred while fetching the active tournament");
            }
        }
    }
}
