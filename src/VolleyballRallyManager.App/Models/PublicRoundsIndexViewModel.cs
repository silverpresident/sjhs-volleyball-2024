using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Models;

/// <summary>
/// ViewModel for the public Rounds index page
/// </summary>
public class PublicRoundsIndexViewModel
{
    public string TournamentName { get; set; } = string.Empty;
    public Guid TournamentId { get; set; }
    public List<DivisionRoundsGroup> DivisionGroups { get; set; } = new List<DivisionRoundsGroup>();
}

/// <summary>
/// Groups rounds by division for display
/// </summary>
public class DivisionRoundsGroup
{
    public Guid DivisionId { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public List<RoundSummaryInfo> Rounds { get; set; } = new List<RoundSummaryInfo>();
}

/// <summary>
/// Summary information for a round card
/// </summary>
public class RoundSummaryInfo
{
    public Guid TournamentRoundId { get; set; }
    public string RoundName { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public bool IsPlayoff { get; set; }
    public MatchGenerationStrategy MatchGenerationStrategy { get; set; }
    public int TeamCount { get; set; }
    public int AdvancingTeamsCount { get; set; }
    public int TotalMatches { get; set; }
    public int CompletedMatches { get; set; }
    public int PendingMatches { get; set; }
    public double CompletionPercentage { get; set; }
    public bool IsFinished { get; set; }
    public bool IsLocked { get; set; }
}
