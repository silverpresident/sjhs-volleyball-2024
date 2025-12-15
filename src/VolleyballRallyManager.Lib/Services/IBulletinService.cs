using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

/// <summary>
/// Service for managing bulletins displayed on public screens.
/// Bulletins provide informational messages to tournament participants and spectators.
/// Supports Markdown formatting for rich content display.
/// </summary>
public interface IBulletinService
{
    /// <summary>
    /// Retrieves a specific bulletin by ID.
    /// </summary>
    /// <param name="id">The bulletin ID to retrieve.</param>
    /// <returns>The bulletin or null if not found.</returns>
    Task<Bulletin?> GetBulletinByIdAsync(Guid id);

    /// <summary>
    /// Gets all bulletins, optionally including hidden ones.
    /// </summary>
    /// <param name="includeHidden">If true, includes hidden bulletins; otherwise only visible bulletins.</param>
    /// <returns>Collection of bulletins.</returns>
    Task<IEnumerable<Bulletin>> GetAllBulletinsAsync(bool includeHidden = false);

    /// <summary>
    /// Creates a new bulletin. Content is processed with Markdown rendering.
    /// </summary>
    /// <param name="bulletin">The bulletin to create.</param>
    /// <returns>The created bulletin with processed HTML content.</returns>
    Task<Bulletin> CreateBulletinAsync(Bulletin bulletin);

    /// <summary>
    /// Updates an existing bulletin. Content is reprocessed with Markdown rendering.
    /// </summary>
    /// <param name="bulletin">The bulletin with updated values.</param>
    /// <returns>The updated bulletin with processed HTML content.</returns>
    Task<Bulletin> UpdateBulletinAsync(Bulletin bulletin);

    /// <summary>
    /// Updates the visibility state of a bulletin without modifying other properties.
    /// </summary>
    /// <param name="id">The bulletin ID.</param>
    /// <param name="isVisible">New visibility state.</param>
    /// <returns>The updated bulletin.</returns>
    Task<Bulletin> UpdateVisibilityAsync(Guid id, bool isVisible);

    /// <summary>
    /// Deletes a bulletin by ID.
    /// </summary>
    /// <param name="id">The bulletin ID to delete.</param>
    /// <returns>True if deleted successfully, false otherwise.</returns>
    Task<bool> DeleteBulletinAsync(Guid id);

    /// <summary>
    /// Gets the most recently created bulletins.
    /// </summary>
    /// <param name="count">Number of recent bulletins to retrieve.</param>
    /// <returns>Collection of recent bulletins, ordered by creation date descending.</returns>
    Task<IEnumerable<Bulletin>> GetRecentAsync(int count);
}
