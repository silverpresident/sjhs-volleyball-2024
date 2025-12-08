namespace VolleyballRallyManager.Lib.Common;

/// <summary>
/// Represents a TournamentEvent event to be processed by the automation worker
/// </summary>
public class TournamentEvent
{
    /// <summary>
    /// The type of scoring event
    /// </summary>
    public TournamentEventType EventType { get; set; }

    /// <summary>
    /// The Tournament this event relates to
    /// </summary>
    public Guid TournamentId { get; set; }
     

    /// <summary>
    /// The team ID 
    /// </summary>
    public Guid? TeamId { get; set; }
    /// <summary>
    /// The Division ID (for changes)
    /// </summary>
    public Guid? DivisionId { get; set; }
    /// <summary>
    /// The Round ID (for changes)
    /// </summary>
    public Guid? RoundId { get; set; }
    /// <summary>
    /// The Tournament Round ID (for changes)
    /// </summary>
    public Guid? TournamentRoundId { get; set; }


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

    public TournamentEvent()
    {
        Timestamp = DateTime.Now;
    }
}
