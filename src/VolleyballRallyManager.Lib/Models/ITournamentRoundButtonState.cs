namespace VolleyballRallyManager.Lib.Models;

/// <summary>
/// Interface for determining button state visibility in tournament round views
/// </summary>
public interface ITournamentRoundButtonState
{
    public int TeamCount { get; set; }
    public int CompletedMatches { get; set; }
    public int TotalMatches { get; set; }
    public bool CanFinalize { get; set; }
    public bool CanSelectTeams { get; set; }
    public bool CanGenerateMatches { get; set; }
    public bool CanGenerateNextRound { get; set; }
    public bool ShowCreatePlayoffRound { get; set; }
}
