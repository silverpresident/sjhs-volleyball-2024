using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.App.Models;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Hubs;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.App.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MatchesController : ApiControllerBase
{
    private readonly IMatchService _matchService;

    public MatchesController(
        ApplicationDbContext context,
        IHubContext<MatchHub> hubContext,
        ILogger<MatchesController> logger,
        IMatchService matchService) 
        : base(context, hubContext, logger)
    {
        _matchService = matchService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Match>>> GetMatches()
    {
        try
        {
            var matches = await _matchService.GetMatchesAsync();
            return Ok(matches);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Match>> GetMatch(Guid id)
    {
        try
        {
            var match = await _matchService.GetMatchAsync(id);
            if (match == null)
            {
                return NotFound();
            }
            return Ok(match);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost]
    public async Task<ActionResult<Match>> CreateMatch(Match match)
    {
        try
        {
            match.CreatedBy = CurrentUserId;
            match.UpdatedBy = CurrentUserId;
            var createdMatch = await _matchService.CreateMatchAsync(match);
            return CreatedAtAction(nameof(GetMatch), new { id = createdMatch.Id }, createdMatch);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPut("{id}/score")]
    public async Task<ActionResult<Match>> UpdateScore(Guid id, [FromBody] ScoreUpdateModel model)
    {
        try
        {
            var match = await _matchService.UpdateMatchScoreAsync(id, model.HomeTeamScore, model.AwayTeamScore, CurrentUserId);
            return Ok(match);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost("{id}/start")]
    public async Task<ActionResult<Match>> StartMatch(Guid id)
    {
        try
        {
            var match = await _matchService.StartMatchAsync(id, CurrentUserId);
            return Ok(match);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost("{id}/finish")]
    public async Task<ActionResult<Match>> FinishMatch(Guid id)
    {
        try
        {
            var match = await _matchService.FinishMatchAsync(id, CurrentUserId);
            return Ok(match);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost("{id}/dispute")]
    public async Task<ActionResult<Match>> RaiseDispute(Guid id)
    {
        try
        {
            var match = await _matchService.GetMatchAsync(id);
            if (match == null)
            {
                return NotFound();
            }

            var disputedMatch = await _matchService.RaiseDisputeAsync(id, CurrentUserId);
            return Ok(disputedMatch);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
}
