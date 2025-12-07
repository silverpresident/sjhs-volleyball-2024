namespace VolleyballRallyManager.Lib.Models;

/// <summary>
/// ViewModel for tournament round details page
/// </summary>
public class TournamentRoundDetailsViewModel: ITournamentRoundButtonState
{
    public TournamentRound CurrentRound { get; set; } = null!;
    public TournamentRound? PreviousRound { get; set; } = null!;
    public List<TournamentRoundTeamSummaryViewModel> Teams { get; set; } = new List<TournamentRoundTeamSummaryViewModel>();
    public List<Match> Matches { get; set; } = new List<Match>(); 
    public string GroupingStrategyLabel { get; set; } = string.Empty;
    public int TeamCount { get; set; }
    public bool CanFinalize { get; set; }
    public bool CanSelectTeams { get; set; }
    public bool CanGenerateMatches { get; set; }
    public bool CanGenerateNextRound { get; set; }
}
