using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

/// <summary>
/// Service for managing tournament round templates/definitions.
/// These RoundTemplate entities serve as templates that define round structures, strategies, and recommendations.
/// </summary>
public interface IRoundService
{
    /// <summary>
    /// Gets all round templates available in the system.
    /// </summary>
    /// <returns>Collection of all rounds.</returns>
    Task<IEnumerable<RoundTemplate>> GetAllRoundsAsync();

    /// <summary>
    /// Retrieves a specific round template by ID.
    /// </summary>
    /// <param name="id">The round ID to retrieve.</param>
    /// <returns>The round or null if not found.</returns>
    Task<RoundTemplate?> GetRoundByIdAsync(Guid id);

    /// <summary>
    /// Gets a round template with its associated matches loaded.
    /// </summary>
    /// <param name="id">The round ID to retrieve.</param>
    /// <returns>The round with matches or null if not found.</returns>
    Task<RoundTemplate?> GetRoundWithMatchesAsync(Guid id);

    /// <summary>
    /// Gets all round templates with their matches loaded.
    /// </summary>
    /// <returns>Collection of rounds with matches.</returns>
    Task<IEnumerable<RoundTemplate>> GetRoundsWithMatchesAsync();

    /// <summary>
    /// Creates a new round template.
    /// </summary>
    /// <param name="round">The round to create.</param>
    Task CreateRoundAsync(RoundTemplate round);

    /// <summary>
    /// Updates an existing round template.
    /// </summary>
    /// <param name="round">The round with updated values.</param>
    Task UpdateRoundAsync(RoundTemplate round);

    /// <summary>
    /// Deletes a round template.
    /// </summary>
    /// <param name="id">The round ID to delete.</param>
    Task DeleteRoundAsync(Guid id);

    /// <summary>
    /// Gets the total number of matches associated with a round.
    /// </summary>
    /// <param name="roundId">The round ID.</param>
    /// <returns>Total match count for this round.</returns>
    Task<int> GetMatchCountForRoundAsync(Guid roundId);

    /// <summary>
    /// Gets the number of completed matches for a round.
    /// </summary>
    /// <param name="roundId">The round ID.</param>
    /// <returns>Completed match count for this round.</returns>
    Task<int> GetCompletedMatchCountForRoundAsync(Guid roundId);
}
