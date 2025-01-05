using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public interface ITeamService
{
    Task<Team?> GetTeamAsync(Guid id);
    Task<IEnumerable<Team>> GetTeamsAsync();
    Task<Team> CreateTeamAsync(Team team);
    Task<Team> UpdateTeamAsync(Team team);
    Task<bool> DeleteTeamAsync(Guid id);
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

}
