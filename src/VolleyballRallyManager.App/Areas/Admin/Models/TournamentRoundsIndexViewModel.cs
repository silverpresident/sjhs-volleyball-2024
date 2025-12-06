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
    public IEnumerable<TournamentRoundSummaryViewModel> Rounds { get; set; } = new List<TournamentRoundSummaryViewModel>();
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
    public TeamSelectionStrategy AdvancingTeamSelectionStrategy { get; set; } = TeamSelectionStrategy.Manual;
    public MatchGenerationStrategy MatchGenerationStrategy { get; set; } = MatchGenerationStrategy.RoundRobin;
    public int AdvancingTeamsCount { get; set; } = 4;
    
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
    public TeamSelectionStrategy SourceSelectionMethod { get; set; }
    public MatchGenerationStrategy SourceMatchStrategy { get; set; }
    
    // DESTINATION SECTION: Teams Advancing to Next Round (Editable)
    [Required]
    [Range(2, int.MaxValue, ErrorMessage = "At least 2 teams must advance")]
    public int AdvancingTeamsCount { get; set; } = 4;
    
    [Required]
    public TeamSelectionStrategy AdvancingTeamSelectionStrategy { get; set; } = TeamSelectionStrategy.TopByPoints;
    
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
    public TeamSelectionStrategy AdvancingTeamSelectionStrategy { get; set; }
    public MatchGenerationStrategy MatchGenerationStrategy { get; set; }
    public int AdvancingTeamsCount { get; set; }
    
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
