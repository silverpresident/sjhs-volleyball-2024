using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

/// <summary>
/// ViewModel for creating a playoff round from best losers
/// </summary>
public class CreatePlayoffRoundViewModel
{
    // Previous RoundTemplate Information
    public Guid PreviousRoundId { get; set; }
    
    [Display(Name = "Previous Round Name")]
    public string PreviousRoundName { get; set; } = string.Empty;
    
    // Tournament Context
    public Guid TournamentId { get; set; }
    
    [Display(Name = "Tournament Name")]
    public string TournamentName { get; set; } = string.Empty;
    
    public Guid DivisionId { get; set; }
    
    [Display(Name = "Division Name")]
    public string DivisionName { get; set; } = string.Empty;
    
    // New Playoff RoundTemplate Configuration
    [Required]
    public Guid RoundId { get; set; }
    
    [ValidateNever]
    [Display(Name = "Playoff Round Name")]
    public string RoundName { get; set; } = string.Empty;
    
    [Required]
    [Range(2, int.MaxValue, ErrorMessage = "Must advance at least 2 teams")]
    [Display(Name = "Teams Advancing from Playoff")]
    public int AdvancingTeamsCount { get; set; } = 2;
    
    [Required]
    [Display(Name = "Advancing Team Selection Strategy")]
    public TeamSelectionStrategy AdvancingTeamSelectionStrategy { get; set; } = TeamSelectionStrategy.TopByPoints;
    
    [Required]
    [Display(Name = "Match Generation Strategy")]
    public MatchGenerationStrategy MatchStrategy { get; set; } = MatchGenerationStrategy.SeededBracket;
    
    [Display(Name = "Group Configuration Type")]
    public GroupGenerationStrategy GroupConfigurationType { get; set; } = GroupGenerationStrategy.NoGroup;
    
    [Display(Name = "Group Configuration Value")]
    [Range(2, int.MaxValue, ErrorMessage = "Minimum value is 2")]
    public int GroupConfigurationValue { get; set; } = 2;
    
    // Team Selection
    [Required]
    [Range(2, int.MaxValue, ErrorMessage = "Must select at least 2 teams")]
    [Display(Name = "Number of Teams to Select")]
    public int NumberOfTeamsToSelect { get; set; } = 4;
    
    [Display(Name = "Candidate Teams (Best Losers)")]
    public List<TournamentRoundTeamSummaryViewModel> CandidateTeams { get; set; } = new();
    
    [Required]
    [MinLength(2, ErrorMessage = "Must select at least 2 teams")]
    [Display(Name = "Selected Teams")]
    public List<Guid> SelectedTeamIds { get; set; } = new();
    
    // Immediate Workflow Execution Flags
    [Display(Name = "Assign teams immediately")]
    public bool AssignTeamsNow { get; set; } = true;
    
    [Display(Name = "Generate matches immediately")]
    public bool GenerateMatchesNow { get; set; } = true;
}