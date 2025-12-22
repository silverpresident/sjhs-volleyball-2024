using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Common;

/// <summary>
/// Represents a TournamentEvent event to be processed by the automation worker
/// </summary>
public class TournamentEvent
{
    /// <summary>
    /// The type of tournament event
    /// </summary>
    public TournamentEventType EventType { get; set; }

    /// <summary>
    /// The Tournament this event relates to
    /// </summary>
    public Guid TournamentId { get; set; }

    /// <summary>
    /// The Match ID (for match-related events)
    /// </summary>
    public Guid? MatchId { get; set; }

    /// <summary>
    /// The set number (for set-related events)
    /// </summary>
    public int? SetNumber { get; set; }

    /// <summary>
    /// The team ID
    /// </summary>
    public Guid? TeamId { get; set; }

    /// <summary>
    /// Score change amount (for score change events)
    /// </summary>
    public int? ScoreChange { get; set; }

    /// <summary>
    /// The Division ID (for changes)
    /// </summary>
    public Guid? DivisionId { get; set; }

    /// <summary>
    /// The RoundTemplate ID (for changes)
    /// </summary>
    public Guid? RoundId { get; set; }

    /// <summary>
    /// The Tournament RoundTemplate ID (for changes)
    /// </summary>
    public Guid? TournamentRoundId { get; set; }

    /// <summary>
    /// Generic entity ID (for bulletins, announcements, etc.)
    /// </summary>
    public Guid? EntityId { get; set; }

    // Complex objects for notification events

    /// <summary>
    /// Match object for match-related notifications
    /// </summary>
    public Match? Match { get; set; }

    /// <summary>
    /// Bulletin object for bulletin notifications
    /// </summary>
    public Bulletin? Bulletin { get; set; }

    /// <summary>
    /// Team object for team notifications
    /// </summary>
    public Team? Team { get; set; }

    /// <summary>
    /// Announcement object for announcement notifications
    /// </summary>
    public Announcement? Announcement { get; set; }

    /// <summary>
    /// MatchUpdate object for feed notifications
    /// </summary>
    public MatchUpdate? MatchUpdate { get; set; }

    /// <summary>
    /// List of announcements for queue change notifications
    /// </summary>
    public List<Announcement>? Announcements { get; set; }

    // Simple notification data

    /// <summary>
    /// Message for general notifications
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Message type (info, warning, error)
    /// </summary>
    public string? MessageType { get; set; }

    /// <summary>
    /// Property name (for property change notifications)
    /// </summary>
    public string? Property { get; set; }

    /// <summary>
    /// Property value (for property change notifications)
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Additional data for the event
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }

    /// <summary>
    /// When the event was created
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Source of the event (e.g., "scorer", "admin", "api")
    /// </summary>
    public string Source { get; set; } = "unknown";

    /// <summary>
    /// User who triggered the event
    /// </summary>
    public string UserName { get; set; } = "unknown";

    public TournamentEvent()
    {
        Timestamp = DateTime.Now;
    }
}
