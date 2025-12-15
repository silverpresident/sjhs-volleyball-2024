using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

/// <summary>
/// Service for generating random team data for testing and development purposes.
/// </summary>
public interface ITeamGenerationService
{
    /// <summary>
    /// Generates a specified number of random teams with fictional names and schools.
    /// Used primarily for testing and demonstration purposes.
    /// </summary>
    /// <param name="count">Number of teams to generate.</param>
    /// <returns>Collection of randomly generated teams.</returns>
    Task<IEnumerable<Team>> GenerateRandomTeamsAsync(int count);
}
