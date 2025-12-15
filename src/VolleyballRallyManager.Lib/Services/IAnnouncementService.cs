using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

/// <summary>
/// Service for managing the tournament announcement queue system.
/// Handles priority-based announcements for calling teams to courts, with support for
/// repeat handling, history tracking, and queue management. See ANNOUNCER_FEATURE.md for details.
/// </summary>
public interface IAnnouncementService
{
    /// <summary>
    /// Gets all announcements, optionally including hidden ones.
    /// </summary>
    /// <param name="includeHidden">If true, includes hidden announcements; otherwise only visible ones.</param>
    /// <returns>Collection of announcements.</returns>
    Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync(bool includeHidden = false);

    /// <summary>
    /// Retrieves a specific announcement by ID.
    /// </summary>
    /// <param name="id">The announcement ID to retrieve.</param>
    /// <returns>The announcement or null if not found.</returns>
    Task<Announcement?> GetAnnouncementByIdAsync(Guid id);

    /// <summary>
    /// Gets all announcements that are queued (not hidden,  not called, or pending recall).
    /// Ordered by priority (Urgent, Info, Routine) and then by sequence number.
    /// </summary>
    /// <returns>Collection of queued announcements in call order.</returns>
    Task<IEnumerable<Announcement>> GetQueuedAnnouncementsAsync();

    /// <summary>
    /// Creates a new announcement and adds it to the queue.
    /// Automatically assigns sequence number based on queue position.
    /// </summary>
    /// <param name="announcement">The announcement to create.</param>
    /// <returns>The created announcement with assigned sequence number.</returns>
    Task<Announcement> CreateAnnouncementAsync(Announcement announcement);

    /// <summary>
    /// Updates an existing announcement.
    /// </summary>
    /// <param name="announcement">The announcement with updated values.</param>
    /// <returns>The updated announcement.</returns>
    Task<Announcement> UpdateAnnouncementAsync(Announcement announcement);

    /// <summary>
    /// Deletes an announcement by ID.
    /// </summary>
    /// <param name="id">The announcement ID to delete.</param>
    /// <returns>True if deleted successfully, false otherwise.</returns>
    Task<bool> DeleteAnnouncementAsync(Guid id);

    /// <summary>
    /// Hides an announcement from the queue without deleting it.
    /// The announcement will no longer appear in the queue but remains in history.
    /// </summary>
    /// <param name="id">The announcement ID to hide.</param>
    /// <returns>The updated announcement with IsHidden = true.</returns>
    Task<Announcement> HideAnnouncementAsync(Guid id);

    /// <summary>
    /// Restores a hidden announcement back to the queue.
    /// </summary>
    /// <param name="id">The announcement ID to unhide.</param>
    /// <returns>The updated announcement with IsHidden = false.</returns>
    Task<Announcement> UnhideAnnouncementAsync(Guid id);

    /// <summary>
    /// Marks an announcement as called/announced.
    /// Records the call in history, increments call count, and handles repeat logic.
    /// If repeats are configured, the announcement is automatically re-queued.
    /// </summary>
    /// <param name="id">The announcement ID to call.</param>
    /// <returns>The updated announcement with call tracking updated.</returns>
    Task<Announcement> CallAnnouncementAsync(Guid id);

    /// <summary>
    /// Defers an announcement to the end of the queue.
    /// Useful when a team is not ready and should be called again later.
    /// Updates sequence number to place announcement at queue end.
    /// </summary>
    /// <param name="id">The announcement ID to defer.</param>
    /// <returns>The updated announcement with new sequence number.</returns>
    Task<Announcement> DeferAnnouncementAsync(Guid id);

    /// <summary>
    /// Manually re-queues an announcement that was previously called.
    /// Resets call status and adds announcement back to the queue.
    /// Different from automatic repeat handling.
    /// </summary>
    /// <param name="id">The announcement ID to re-announce.</param>
    /// <returns>The updated announcement restored to queue.</returns>
    Task<Announcement> ReannounceAsync(Guid id);

    /// <summary>
    /// Gets the call history for a specific announcement.
    /// Shows all times the announcement was called with timestamps.
    /// </summary>
    /// <param name="id">The announcement ID.</param>
    /// <returns>Collection of history log entries for this announcement.</returns>
    Task<IEnumerable<AnnouncementHistoryLog>> GetHistoryForAnnouncementAsync(Guid id);

    /// <summary>
    /// Checks if an announcement with the specified title already exists.
    /// Used for duplicate prevention.
    /// </summary>
    /// <param name="title">The title to check.</param>
    /// <returns>True if an announcement with this title exists, false otherwise.</returns>
    Task<bool> TitleExistsAsync(string title);
}
