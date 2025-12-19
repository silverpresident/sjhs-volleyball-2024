using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

/// <summary>
/// Service for sending real-time notifications via SignalR to connected clients.
/// Broadcasts tournament events to public displays, admin dashboards, and mobile apps.
/// All notification methods are fire-and-forget with no return values.
/// </summary>
public interface ISignalRNotificationService
{
    /// <summary>
    /// Notifies clients that a new match has been created.
    /// </summary>
    /// <param name="match">The newly created match.</param>
    Task NotifyMatchCreatedAsync(Match match);

    /// <summary>
    /// Notifies clients that match details have been updated.
    /// </summary>
    /// <param name="match">The updated match.</param>
    Task NotifyMatchUpdatedAsync(Match match);

    /// <summary>
    /// Notifies clients that a match has started.
    /// </summary>
    /// <param name="match">The match that started.</param>
    Task NotifyMatchStartedAsync(Match match);

    /// <summary>
    /// Notifies clients that a match has finished with final scores.
    /// </summary>
    /// <param name="match">The finished match.</param>
    Task NotifyMatchFinishedAsync(Match match);

    /// <summary>
    /// Notifies clients that a dispute has been raised for a match.
    /// </summary>
    /// <param name="match">The disputed match.</param>
    Task NotifyMatchDisputedAsync(Match match);

    /// <summary>
    /// Notifies clients of a score update during a match.
    /// Sent in real-time as points are scored.
    /// </summary>
    /// <param name="match">The match with updated scores.</param>
    Task NotifyScoreUpdateAsync(Match match);

    /// <summary>
    /// Notifies clients that a new bulletin has been posted.
    /// </summary>
    /// <param name="bulletin">The newly created bulletin.</param>
    Task NotifyBulletinCreatedAsync(Bulletin bulletin);

    /// <summary>
    /// Notifies clients that a bulletin has been updated.
    /// </summary>
    /// <param name="bulletin">The updated bulletin.</param>
    Task NotifyBulletinUpdatedAsync(Bulletin bulletin);

    /// <summary>
    /// Notifies clients that a bulletin has been deleted.
    /// </summary>
    /// <param name="bulletinId">The ID of the deleted bulletin.</param>
    Task NotifyBulletinDeletedAsync(Guid bulletinId);

    /// <summary>
    /// Notifies clients that a new team has been added.
    /// </summary>
    /// <param name="team">The newly created team.</param>
    Task NotifyTeamCreatedAsync(Team team);

    /// <summary>
    /// Notifies clients that team information has been updated.
    /// </summary>
    /// <param name="team">The updated team.</param>
    Task NotifyTeamUpdatedAsync(Team team);

    /// <summary>
    /// Notifies clients that a team has been deleted.
    /// </summary>
    /// <param name="teamId">The ID of the deleted team.</param>
    Task NotifyTeamDeletedAsync(Guid teamId);

    /// <summary>
    /// Notifies clients that a tournament round has started.
    /// </summary>
    /// <param name="round">The round that started.</param>
    Task NotifyRoundStartedAsync(RoundTemplate round);

    /// <summary>
    /// Notifies clients that a tournament round has finished.
    /// </summary>
    /// <param name="round">The finished round.</param>
    Task NotifyRoundFinishedAsync(RoundTemplate round);

    /// <summary>
    /// Notifies clients of a tournament status change.
    /// </summary>
    /// <param name="status">Status message describing the tournament state.</param>
    Task NotifyTournamentStatusAsync(string status);

    /// <summary>
    /// Notifies clients of an error or issue.
    /// </summary>
    /// <param name="error">Error message to display.</param>
    Task NotifyErrorAsync(string error);

    /// <summary>
    /// Broadcasts a general message to all connected clients.
    /// </summary>
    /// <param name="message">The message to broadcast.</param>
    /// <param name="type">Message type (info, warning, error). Default is "info".</param>
    Task BroadcastMessageAsync(string message, string type = "info");

    /// <summary>
    /// Notifies clients of a new match update/event in the feed.
    /// Updates appear in the live feed on public displays.
    /// </summary>
    /// <param name="update">The match update event.</param>
    Task NotifyAddFeedAsync(MatchUpdate update);

    /// <summary>
    /// Notifies clients that a new announcement has been created.
    /// </summary>
    /// <param name="announcement">The newly created announcement.</param>
    Task NotifyAnnouncementCreatedAsync(Announcement announcement);

    /// <summary>
    /// Notifies clients that an announcement has been updated.
    /// </summary>
    /// <param name="announcement">The updated announcement.</param>
    Task NotifyAnnouncementUpdatedAsync(Announcement announcement);

    /// <summary>
    /// Notifies clients that an announcement has been deleted.
    /// </summary>
    /// <param name="announcementId">The ID of the deleted announcement.</param>
    Task NotifyAnnouncementDeletedAsync(Guid announcementId);

    /// <summary>
    /// Notifies clients that a specific property of an announcement has changed.
    /// Allows for targeted UI updates without full announcement refresh.
    /// </summary>
    /// <param name="announcementId">The announcement ID.</param>
    /// <param name="property">The property name that changed.</param>
    /// <param name="value">The new value of the property.</param>
    Task NotifyAnnouncementPropertyChangedAsync(Guid announcementId, string property, string value);

    /// <summary>
    /// Notifies clients that the announcement queue has changed.
    /// Sends the updated queue order to all connected clients.
    /// </summary>
    /// <param name="announcement">List of announcements in new queue order.</param>
    Task NotifyAnnouncementQueueChangedAsync(List<Announcement> announcement);

    /// <summary>
    /// Notifies clients that an announcement has been called/announced.
    /// Triggers audio/visual alerts on public displays.
    /// </summary>
    /// <param name="announcement">The announcement that was called.</param>
    Task NotifyAnnouncementCalledAsync(Announcement announcement);
}
