namespace VolleyballRallyManager.Lib.Models;

/// <summary>
/// </summary>
public interface ITournamentRoundButtonState
{
    public int TeamCount { get; set; }
    public bool CanFinalize { get; set; }
    public bool CanSelectTeams { get; set; }
    public bool CanGenerateMatches { get; set; }
    public bool CanGenerateNextRound { get; set; }
}
