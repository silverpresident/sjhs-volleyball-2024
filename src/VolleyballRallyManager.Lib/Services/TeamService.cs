using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

/// <summary>
/// Service for managing teams in the volleyball tournament system.
/// Handles basic CRUD operations for team entities.
/// </summary>
public interface ITeamService
{
    /// <summary>
    /// Retrieves a specific team by ID.
    /// </summary>
    /// <param name="id">The team ID to retrieve.</param>
    /// <returns>The team or null if not found.</returns>
    Task<Team?> GetTeamAsync(Guid id);

    /// <summary>
    /// Gets all teams in the system, ordered by name.
    /// </summary>
    /// <returns>Collection of all teams.</returns>
    Task<IEnumerable<Team>> GetTeamsAsync();

    /// <summary>
    /// Creates a new team.
    /// </summary>
    /// <param name="team">The team to create.</param>
    /// <returns>The created team.</returns>
    Task<Team> CreateTeamAsync(Team team);

    /// <summary>
    /// Updates an existing team.
    /// </summary>
    /// <param name="team">The team with updated values.</param>
    /// <returns>The updated team.</returns>
    /// <exception cref="InvalidOperationException">Thrown if team is not found.</exception>
    Task<Team> UpdateTeamAsync(Team team);

    /// <summary>
    /// Deletes a team by ID.
    /// Teams with existing matches cannot be deleted.
    /// </summary>
    /// <param name="id">The team ID to delete.</param>
    /// <returns>True if deleted successfully, false if team not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if team has existing matches.</exception>
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
