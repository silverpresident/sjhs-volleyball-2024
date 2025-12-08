namespace VolleyballRallyManager.Lib.Common;

/// <summary>
/// Represents the type of scoring event that occurred
/// </summary>
public enum ScoringEventType
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
    MatchDisputed
}
