using VolleyballRallyManager.Lib.Models;
using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

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
    public string TournamentName { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public string RoundName { get; set; } = string.Empty;
    public int TotalTeamsInDivision { get; set; }
    public TeamSelectionMethod TeamSelectionMethod { get; set; } = TeamSelectionMethod.Manual;
    public MatchGenerationStrategy MatchGenerationStrategy { get; set; } = MatchGenerationStrategy.RoundRobin;
    public int TeamsAdvancing { get; set; } = 4;
    
    // Group Configuration
    public string GroupConfigurationType { get; set; } = "TeamsPerGroup"; // "TeamsPerGroup" or "GroupsInRound"
    public int GroupConfigurationValue { get; set; } = 2;
    
    // Post-action flags
    public bool AssignTeamsNow { get; set; } = true;
    public bool GenerateMatchesNow { get; set; } = true;
}

/// <summary>
/// ViewModel for creating the next round
/// </summary>
public class CreateNextRoundViewModel
{
    // Identification
    public Guid TournamentId { get; set; }
    public Guid DivisionId { get; set; }
    public Guid RoundId { get; set; }
    public Guid PreviousTournamentRoundId { get; set; }
    
    // Display Information
    public string TournamentName { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public string PreviousRoundName { get; set; } = string.Empty;
    public Guid CurrentRoundId { get; set; } // To disable in dropdown
    
    // SOURCE SECTION: Teams Coming Into This Round (Read-Only/Pre-populated)
    public int SourceTeamCount { get; set; }
    public TeamSelectionMethod SourceSelectionMethod { get; set; }
    public MatchGenerationStrategy SourceMatchStrategy { get; set; }
    
    // DESTINATION SECTION: Teams Advancing to Next Round (Editable)
    [Required]
    [Range(2, int.MaxValue, ErrorMessage = "At least 2 teams must advance")]
    public int TeamsAdvancing { get; set; } = 4;
    
    [Required]
    public TeamSelectionMethod TeamSelectionMethod { get; set; } = TeamSelectionMethod.TopByPoints;
    
    [Required]
    public MatchGenerationStrategy MatchGenerationStrategy { get; set; } = MatchGenerationStrategy.SeededBracket;
    
    // Group Configuration for the NEW round
    public string GroupConfigurationType { get; set; } = "TeamsPerGroup";
    public int GroupConfigurationValue { get; set; } = 2;
    
    // Post-action flags (Immediate Workflow Execution)
    public bool AssignTeamsNow { get; set; } = true;
    public bool GenerateMatchesNow { get; set; } = true;
}

/// <summary>
/// ViewModel for generating matches
/// </summary>
public class GenerateMatchesViewModel
{
    public Guid TournamentRoundId { get; set; }
    public string RoundName { get; set; } = string.Empty;
    
    [DataType(DataType.Time)] 
    [DisplayFormat(DataFormatString = "{0:HH\\:mm}", ApplyFormatInEditMode = true)]
    public DateTime StartTime { get; set; } = DateTime.Now;
    public int StartingCourtNumber { get; set; } = 1;
    public int NumberOfCourts { get; set; } = 1;
    public int MatchTimeInterval { get; set; } = 10;
    public int TeamCount { get; set; }
    public MatchGenerationStrategy Strategy { get; set; }
}

/// <summary>
/// ViewModel for editing a tournament round
/// </summary>
public class EditTournamentRoundViewModel
{
    public Guid Id { get; set; }
    public Guid TournamentId { get; set; }
    public Guid DivisionId { get; set; }
    public Guid RoundId { get; set; }
    public string TournamentName { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public string RoundName { get; set; } = string.Empty;
    public int TotalTeamsInDivision { get; set; }
    public int RoundNumber { get; set; }
    public TeamSelectionMethod TeamSelectionMethod { get; set; }
    public MatchGenerationStrategy MatchGenerationStrategy { get; set; }
    public int TeamsAdvancing { get; set; }
    
    // Group Configuration
    public string GroupConfigurationType { get; set; } = "TeamsPerGroup"; // "TeamsPerGroup" or "GroupsInRound"
    public int GroupConfigurationValue { get; set; } = 2;
    
    // Post-action flags
    public bool AssignTeamsNow { get; set; } = false;
    public bool GenerateMatchesNow { get; set; } = false;
    
    // State flags
    public bool IsFinished { get; set; }
    public bool HasTeams { get; set; }
    public bool HasMatches { get; set; }
}
