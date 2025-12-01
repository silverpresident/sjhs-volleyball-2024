using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

/// <summary>
/// ViewModel for displaying a tournament round in the index
/// </summary>
public class TournamentRoundViewModel
{
    public Guid Id { get; set; }
    public string TournamentName { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public string RoundName { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public int TeamCount { get; set; }
    public int MatchCount { get; set; }
    public int CompletedMatchCount { get; set; }
    public bool IsFinished { get; set; }
    public bool IsLocked { get; set; }
    public TeamSelectionMethod TeamSelectionMethod { get; set; }
    public MatchGenerationStrategy MatchGenerationStrategy { get; set; }
    public int TeamsAdvancing { get; set; }
    
    // Conditional button visibility flags
    public bool CanFinalize { get; set; }
    public bool CanSelectTeams { get; set; }
    public bool CanGenerateMatches { get; set; }
    public bool CanGenerateNextRound { get; set; }
}

/// <summary>
/// ViewModel for the tournament rounds index page
/// </summary>
public class TournamentRoundsIndexViewModel
{
    public Guid TournamentId { get; set; }
    public Guid DivisionId { get; set; }
    public string TournamentName { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public IEnumerable<TournamentRoundViewModel> Rounds { get; set; } = new List<TournamentRoundViewModel>();
}

/// <summary>
/// ViewModel for tournament round details page
/// </summary>
public class TournamentRoundDetailsViewModel
{
    public TournamentRound Round { get; set; } = null!;
    public List<TournamentRoundTeamDetailsViewModel> Teams { get; set; } = new List<TournamentRoundTeamDetailsViewModel>();
    public List<Match> Matches { get; set; } = new List<Match>();
    public bool CanFinalize { get; set; }
    public bool CanSelectTeams { get; set; }
    public bool CanGenerateMatches { get; set; }
    public bool CanGenerateNextRound { get; set; }
}

/// <summary>
/// ViewModel for displaying team details in a round
/// </summary>
public class TournamentRoundTeamDetailsViewModel
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int SeedNumber { get; set; }
    public int FinalRank { get; set; }
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

/// <summary>
/// ViewModel for creating the first round
/// </summary>
public class CreateFirstRoundViewModel
{
    public Guid TournamentId { get; set; }
    public Guid DivisionId { get; set; }
    public Guid RoundId { get; set; }
    public TeamSelectionMethod TeamSelectionMethod { get; set; } = TeamSelectionMethod.Manual;
    public MatchGenerationStrategy MatchGenerationStrategy { get; set; } = MatchGenerationStrategy.RoundRobin;
    public int TeamsAdvancing { get; set; } = 4;
}

/// <summary>
/// ViewModel for creating the next round
/// </summary>
public class CreateNextRoundViewModel
{
    public Guid TournamentId { get; set; }
    public Guid DivisionId { get; set; }
    public Guid RoundId { get; set; }
    public Guid PreviousTournamentRoundId { get; set; }
    public string PreviousRoundName { get; set; } = string.Empty;
    public TeamSelectionMethod TeamSelectionMethod { get; set; } = TeamSelectionMethod.TopByPoints;
    public MatchGenerationStrategy MatchGenerationStrategy { get; set; } = MatchGenerationStrategy.SeededBracket;
    public int TeamsAdvancing { get; set; } = 4;
}

/// <summary>
/// ViewModel for generating matches
/// </summary>
public class GenerateMatchesViewModel
{
    public Guid TournamentRoundId { get; set; }
    public string RoundName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.Now;
    public string CourtLocation { get; set; } = "Court 1";
    public int TeamCount { get; set; }
    public MatchGenerationStrategy Strategy { get; set; }
}
