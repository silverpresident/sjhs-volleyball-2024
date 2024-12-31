using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Hubs;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.Admin.Controllers;

[Authorize]
public class TeamsController : ApiControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(
        ApplicationDbContext context,
        IHubContext<MatchHub> hubContext,
        ITeamService teamService,
        ILogger<TeamsController> logger)
        : base(context, hubContext, logger)
    {
        _teamService = teamService;
    }

    [HttpGet("")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
    {
        try
        {
            var teams = await _teamService.GetTeamsAsync();
            return Ok(teams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teams");
            return StatusCode(500, "An error occurred while retrieving teams");
        }
    }

    [HttpGet("leaderboard")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Team>>> GetLeaderboard([FromQuery] Division? division = null)
    {
        try
        {
            var teams = await _teamService.GetLeaderboardAsync(division.Id);
            return Ok(teams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard");
            return StatusCode(500, "An error occurred while retrieving the leaderboard");
        }
    }

    [HttpGet("division/{division}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Team>>> GetTeamsByDivision(Division division)
    {
        try
        {
            var teams = await _teamService.GetTeamsByDivisionAsync(division);
            return Ok(teams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teams for division {Division}", division);
            return StatusCode(500, "An error occurred while retrieving teams");
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Team>> GetTeam(Guid id)
    {
        try
        {
            var team = await _teamService.GetTeamAsync(id);

            if (team == null)
            {
                return NotFound($"Team with ID {id} not found.");
            }

            return Ok(team);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team {TeamId}", id);
            return StatusCode(500, "An error occurred while retrieving the team");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Team>> CreateTeam(Team team)
    {
        try
        {
            var createdTeam = await _teamService.CreateTeamAsync(team);
            await _hubContext.Clients.All.SendAsync("TeamUpdated", createdTeam);

            return CreatedAtAction(nameof(GetTeam), new { id = createdTeam.Id }, createdTeam);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating team");
            return StatusCode(500, "An error occurred while creating the team");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Team>> UpdateTeam(Guid id, Team team)
    {
        if (id != team.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var updatedTeam = await _teamService.UpdateTeamAsync(team);
            await _hubContext.Clients.All.SendAsync("TeamUpdated", updatedTeam);

            return Ok(updatedTeam);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team {TeamId}", id);
            return StatusCode(500, "An error occurred while updating the team");
        }
    }

    [HttpPut("{id}/recalculate")]
    public async Task<ActionResult<Team>> RecalculateTeamStatistics(Guid id)
    {
        try
        {
            await _teamService.RecalculateTeamStatisticsAsync(id);
            var team = await _teamService.GetTeamAsync(id);

            if (team == null)
            {
                return NotFound($"Team with ID {id} not found.");
            }

            await _hubContext.Clients.All.SendAsync("TeamUpdated", team);
            return Ok(team);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating statistics for team {TeamId}", id);
            return StatusCode(500, "An error occurred while recalculating team statistics");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Team>> DeleteTeam(Guid id)
    {
        try
        {
            var success = await _teamService.DeleteTeamAsync(id);
            if (!success)
            {
                return NotFound($"Team with ID {id} not found.");
            }

            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting team {TeamId}", id);
            return StatusCode(500, "An error occurred while deleting the team");
        }
    }
}
