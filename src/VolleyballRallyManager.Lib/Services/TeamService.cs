using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public interface ITeamService
{
    Task<Team?> GetTeamAsync(Guid id);
    Task<IEnumerable<Team>> GetTeamsAsync();
    Task<IEnumerable<Team>> GetTeamsByDivisionAsync(Division division);
    Task<Team> CreateTeamAsync(Team team);
    Task<Team> UpdateTeamAsync(Team team);
    Task<bool> DeleteTeamAsync(Guid id);
    Task<IEnumerable<Team>> GetLeaderboardAsync(Guid divisionId);
    Task RecalculateTeamStatisticsAsync(Guid teamId);
}

public class TeamService : ITeamService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TeamService> _logger;

    public TeamService(
        ApplicationDbContext context,
        ILogger<TeamService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Team?> GetTeamAsync(Guid id)
    {
        return await _context.Teams.FindAsync(id);
    }

    public async Task<IEnumerable<Team>> GetTeamsAsync()
    {
        return await _context.Teams
            .OrderBy(t => t.Division)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Team>> GetTeamsByDivisionAsync(Division division)
    {
        return await _context.Teams
            .Where(t => t.Division == division)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Team> CreateTeamAsync(Team team)
    {
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return team;
    }

    public async Task<Team> UpdateTeamAsync(Team team)
    {
        var existingTeam = await GetTeamAsync(team.Id) ??
            throw new InvalidOperationException($"Team with ID {team.Id} not found.");

        _context.Entry(existingTeam).CurrentValues.SetValues(team);
        await _context.SaveChangesAsync();

        return team;
    }

    public async Task<bool> DeleteTeamAsync(Guid id)
    {
        var team = await GetTeamAsync(id);
        if (team == null)
        {
            return false;
        }

        // Check if team has any matches
        var hasMatches = await _context.Matches
            .AnyAsync(m => m.HomeTeamId == id || m.AwayTeamId == id);

        if (hasMatches)
        {
            throw new InvalidOperationException("Cannot delete team with existing matches.");
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Team>> GetLeaderboardAsync(Guid divisionId)
    {
        var query = _context.Teams.AsQueryable();

        if (divisionId != Guid.Empty)
        {
            query = query.Where(t => t.Division.Id == divisionId);
        }

        return await query
            .OrderByDescending(t => t.TotalPoints)
            .ThenByDescending(t => t.PointDifference)
            .ThenByDescending(t => t.PointsScored)
            .ToListAsync();
    }

    public async Task RecalculateTeamStatisticsAsync(Guid teamId)
    {
        var team = await _context.Teams.FindAsync(teamId) ??
            throw new InvalidOperationException($"Team with ID {teamId} not found.");

        // Reset statistics
        team.MatchesPlayed = 0;
        team.Wins = 0;
        team.Draws = 0;
        team.Losses = 0;
        team.PointsScored = 0;
        team.PointsConceded = 0;
        team.TotalPoints = 0;

        // Get all finished matches for this team
        var matches = await _context.Matches
            .Where(m => (m.HomeTeamId == teamId || m.AwayTeamId == teamId) && m.IsFinished)
            .ToListAsync();

        foreach (var match in matches)
        {
            team.MatchesPlayed++;

            if (match.HomeTeamId == teamId)
            {
                team.PointsScored += match.HomeTeamScore;
                team.PointsConceded += match.AwayTeamScore;

                if (match.HomeTeamScore > match.AwayTeamScore)
                {
                    team.Wins++;
                    team.TotalPoints += 3;
                }
                else if (match.HomeTeamScore < match.AwayTeamScore)
                {
                    team.Losses++;
                }
                else
                {
                    team.Draws++;
                    team.TotalPoints += 1;
                }
            }
            else // Away team
            {
                team.PointsScored += match.AwayTeamScore;
                team.PointsConceded += match.HomeTeamScore;

                if (match.AwayTeamScore > match.HomeTeamScore)
                {
                    team.Wins++;
                    team.TotalPoints += 3;
                }
                else if (match.AwayTeamScore < match.HomeTeamScore)
                {
                    team.Losses++;
                }
                else
                {
                    team.Draws++;
                    team.TotalPoints += 1;
                }
            }
        }

        await _context.SaveChangesAsync();
    }
}
