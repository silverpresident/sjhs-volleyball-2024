using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VolleyballRallyManager.Lib.Models;

/// <summary>
/// Represents a round within a specific tournament division
/// Rounds are sequenced independently within each division
/// </summary>
public class TournamentRound : BaseEntity
{
    // Primary Identifiers
    [Required]
    [Display(Name = "Tournament")]
    public Guid TournamentId { get; set; }
    
    [ForeignKey("TournamentId")]
    public Tournament Tournament { get; set; } = null!;

    [Required]
    [Display(Name = "Division")]
    public Guid DivisionId { get; set; }
    
    [ForeignKey("DivisionId")]
    public Division Division { get; set; } = null!;

    [Required]
    [Display(Name = "Round")]
    public Guid RoundTemplateId { get; set; }
    
    [ForeignKey("RoundTemplateId")]
    public RoundTemplate Round { get; set; } = null!;

    // CurrentRound Configuration
    [Required]
    [Display(Name = "Round Number")]
    public int RoundNumber { get; set; }

    [Required]
    [Display(Name = "Team Selection Method")]
    public TeamSelectionStrategy AdvancingTeamSelectionStrategy { get; set; } = TeamSelectionStrategy.Manual;

    [Required]
    [Display(Name = "Match Generation Strategy")]
    //This is for the current round
    public MatchGenerationStrategy MatchGenerationStrategy { get; set; } = MatchGenerationStrategy.Manual;

    [Required]
    [Display(Name = "Match Generation Strategy")]
    //This is for the current round
    public GroupGenerationStrategy GroupingStrategy { get; set; } = GroupGenerationStrategy.NoGroup;
     
    [Display(Name = "Previous Round")]
    public Guid? PreviousTournamentRoundId { get; set; }
    [Display(Name = "Next Round")]
    public Guid? NextTournamentRoundId { get; set; }
    
    [Required]
    [Display(Name = "Teams Advancing")]
    public int AdvancingTeamsCount { get; set; } = 0;

    /// <summary>
    /// The number of teams expected to qualify for this round from previous rounds or seeding.
    /// </summary>
    [Display(Name = "Qualifying Teams Count")]
    public int QualifyingTeamsCount { get; set; } = 0;

    /// <summary>
    /// The strategy used to select teams that qualify for this round.
    /// </summary>
    [Display(Name = "Qualifying Team Selection Strategy")]
    public TeamSelectionStrategy QualifyingTeamSelectionStrategy { get; set; } = TeamSelectionStrategy.Manual;

    /// <summary>
    /// Indicates whether this round is a playoff round, which may influence ranking and progression.
    /// </summary>
    [Display(Name = "Is Playoff")]
    public bool IsPlayoff { get; set; } = false;

    [Display(Name = "Teams Per Group")]
    public int? TeamsPerGroup { get; set; }

    [Display(Name = "Groups In Round")]
    public int? GroupsInRound { get; set; }

    // State Flags
    [Display(Name = "Is Finished")]
    public bool IsFinished { get; set; } = false;

    [Display(Name = "Is Locked")]
    public bool IsLocked { get; set; } = false;

    // Navigation Properties
    public virtual ICollection<TournamentRoundTeam> TournamentRoundTeams { get; set; } = new List<TournamentRoundTeam>();
}
