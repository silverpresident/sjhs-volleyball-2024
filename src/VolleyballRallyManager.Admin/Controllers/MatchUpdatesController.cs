using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Admin.Models;
using VolleyballRallyManager.Lib.Hubs;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.Admin.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MatchUpdatesController : ControllerBase
{
    private readonly IMatchService _matchService;
    private readonly IHubContext<MatchHub> _hubContext;

    public MatchUpdatesController(IMatchService matchService, IHubContext<MatchHub> hubContext)
    {
        _matchService = matchService;
        _hubContext = hubContext;
    }

    [HttpGet("{matchId}")]
    public async Task<ActionResult<List<MatchUpdate>>> GetMatchUpdates(Guid matchId)
    {
        var updates = await _matchService.GetMatchUpdatesAsync(matchId);
        return Ok(updates);
    }

    [HttpPost]
    public async Task<ActionResult<MatchUpdate>> CreateUpdate([FromBody] MatchUpdate update)
    {
        try
        {
            var result = await _matchService.AddMatchUpdateAsync(update);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("score")]
    public async Task<ActionResult<Match>> UpdateScore([FromBody] ScoreUpdateModel model)
    {
        try
        {
            var match = await _matchService.UpdateScoreAsync(
                model.MatchId,
                model.HomeTeamScore,
                model.AwayTeamScore,
                model.IsFinished,
                model.IsDisputed,
                model.Notes
            );

            if (model.IsFinished)
            {
                await _matchService.UpdateTeamStatisticsAsync(model.MatchId);
            }

            return Ok(match);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Match not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/start")]
    public async Task<ActionResult<Match>> StartMatch(Guid id)
    {
        try
        {
            var match = await _matchService.StartMatchAsync(id);
            return Ok(match);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Match not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/finish")]
    public async Task<ActionResult<Match>> FinishMatch(Guid id)
    {
        try
        {
            var match = await _matchService.FinishMatchAsync(id);
            await _matchService.UpdateTeamStatisticsAsync(id);
            return Ok(match);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Match not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/dispute")]
    public async Task<ActionResult<Match>> DisputeMatch(Guid id, [FromBody] string reason)
    {
        try
        {
            var match = await _matchService.DisputeMatchAsync(id, reason);
            return Ok(match);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Match not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/referee")]
    public async Task<ActionResult<Match>> AssignReferee(Guid id, [FromBody] string refereeName)
    {
        try
        {
            var match = await _matchService.AssignRefereeAsync(id, refereeName);
            return Ok(match);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Match not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/scorer")]
    public async Task<ActionResult<Match>> AssignScorer(Guid id, [FromBody] string scorerName)
    {
        try
        {
            var match = await _matchService.AssignScorerAsync(id, scorerName);
            return Ok(match);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Match not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
