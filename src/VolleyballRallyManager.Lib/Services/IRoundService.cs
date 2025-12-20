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
    /// Creates a new round template.
    /// </summary>
    /// <param name="round">The round to create.</param>
    Task CreateRoundAsync(RoundTemplate round);

    /// <summary>
    /// Updates an existing round template.
    /// </summary>
    /// <param name="round">The round with updated values.</param>
    Task UpdateRoundAsync(RoundTemplate round);
     
}
