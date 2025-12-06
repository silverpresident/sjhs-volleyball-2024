namespace VolleyballRallyManager.Lib.Models;

/// <summary>
/// Represents a scoring event to be processed by the automation worker
/// </summary>
public class ScoringEvent
{
    /// <summary>
    /// The type of scoring event
    /// </summary>
    public ScoringEventType EventType { get; set; }

    /// <summary>
    /// The match this event relates to
    /// </summary>
    public Guid MatchId { get; set; }

    /// <summary>
    /// The set number (if applicable to the event)
    /// </summary>
    public int? SetNumber { get; set; }

    /// <summary>
    /// The team ID (for score changes)
    /// </summary>
    public Guid? TeamId { get; set; }

    /// <summary>
    /// The score change value (positive or negative)
    /// </summary>
    public int? ScoreChange { get; set; }

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
    /// Source of the event (e.g., "scorer", "admin", "api")
    /// </summary>
    public string UserName { get; set; } = "unknown";

    public ScoringEvent()
    {
        Timestamp = DateTime.Now;
    }
}
