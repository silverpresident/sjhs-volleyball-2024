namespace VolleyballRallyManager.Lib.Models;

/// <summary>
/// ViewModel for displaying team details in a round
/// </summary>
public class TournamentRoundTeamSummaryViewModel
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int SeedNumber { get; set; }
    public int Rank { get; set; }
    public int RankingPoints { get; set; }
    public int Points { get; set; }
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int SetsFor { get; set; }
    public int SetsAgainst { get; set; }
    public int SetsDifference { get; set; }
    public int ScoreFor { get; set; }
    public int ScoreAgainst { get; set; }
    public int ScoreDifference { get; set; }
    public string GroupName { get; set; } = string.Empty;
}
