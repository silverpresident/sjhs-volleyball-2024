namespace VolleyballRallyManager.Lib.Common;

/// <summary>
/// Represents the type of tournament event that occurred
/// </summary>
public enum TournamentEventType
{
    /// <summary>
    /// Teams called to court for their match
    /// </summary>
    CallToCourt,

    /// <summary>
    /// Request for support/admin assistance
    /// </summary>
    CallToSupport,

    /// <summary>
    /// Match has started
    /// </summary>
    MatchStart,

    /// <summary>
    /// A new set has started
    /// </summary>
    MatchSetStart,

    /// <summary>
    /// Current set has ended
    /// </summary>
    MatchSetEnd,

    /// <summary>
    /// Revert to the previous set
    /// </summary>
    MatchSetRevertToPrevious,

    /// <summary>
    /// Score change in the current set
    /// </summary>
    MatchSetScoreChange,

    /// <summary>
    /// Match has ended
    /// </summary>
    MatchEnd,

    /// <summary>
    /// Match has been disputed
    /// </summary>
    MatchDisputed,

    /// <summary>
    /// Update division rankings
    /// </summary>
    UpdateDivisionRanks,

    // Notification Events
    
    /// <summary>
    /// Notify clients that a match has been created
    /// </summary>
    MatchCreated,

    /// <summary>
    /// Notify clients that a match has been updated
    /// </summary>
    MatchUpdated,

    /// <summary>
    /// Notify clients that a match has started
    /// </summary>
    MatchStarted,

    /// <summary>
    /// Notify clients that a match has finished
    /// </summary>
    MatchFinished,

    /// <summary>
    /// Notify clients that a match has been disputed
    /// </summary>
    MatchDisputedNotification,

    /// <summary>
    /// Notify clients of a score update
    /// </summary>
    ScoreUpdate,

    /// <summary>
    /// Notify clients that a bulletin has been created
    /// </summary>
    BulletinCreated,

    /// <summary>
    /// Notify clients that a bulletin has been updated
    /// </summary>
    BulletinUpdated,

    /// <summary>
    /// Notify clients that a bulletin has been deleted
    /// </summary>
    BulletinDeleted,

    /// <summary>
    /// Notify clients that a team has been created
    /// </summary>
    TeamCreated,

    /// <summary>
    /// Notify clients that a team has been updated
    /// </summary>
    TeamUpdated,

    /// <summary>
    /// Notify clients that a team has been deleted
    /// </summary>
    TeamDeleted,

    /// <summary>
    /// Notify clients of a tournament status change
    /// </summary>
    TournamentStatus,

    /// <summary>
    /// Notify clients of an error
    /// </summary>
    ErrorNotification,

    /// <summary>
    /// Broadcast a general message to all clients
    /// </summary>
    BroadcastMessage,

    /// <summary>
    /// Notify clients of a new match update in the feed
    /// </summary>
    AddFeed,

    /// <summary>
    /// Notify clients that an announcement has been created
    /// </summary>
    AnnouncementCreated,

    /// <summary>
    /// Notify clients that an announcement has been updated
    /// </summary>
    AnnouncementUpdated,

    /// <summary>
    /// Notify clients that an announcement has been deleted
    /// </summary>
    AnnouncementDeleted,

    /// <summary>
    /// Notify clients that an announcement property has changed
    /// </summary>
    AnnouncementPropertyChanged,

    /// <summary>
    /// Notify clients that the announcement queue has changed
    /// </summary>
    AnnouncementQueueChanged,

    /// <summary>
    /// Notify clients that an announcement has been called
    /// </summary>
    AnnouncementCalled
}
