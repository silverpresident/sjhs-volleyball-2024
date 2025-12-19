using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

/// <summary>
/// Simplified ViewModel for "Create Next RoundTemplate" feature
/// Only requires round type selection; defaults are applied from RoundTemplate template
/// </summary>
public class CreateNextRoundSimpleViewModel
{
    // Context Information (read-only display)
    public Guid TournamentId { get; set; }
    public Guid DivisionId { get; set; }
    public Guid PreviousTournamentRoundId { get; set; }
    
    [Display(Name = "Tournament")]
    [ValidateNever]
    public string TournamentName { get; set; } = string.Empty;
    
    [Display(Name = "Division")]
    [ValidateNever]
    public string DivisionName { get; set; } = string.Empty;
    
    [Display(Name = "Previous Round")]
    [ValidateNever]
    public string PreviousRoundName { get; set; } = string.Empty;
    
    // User Input (only field to configure)
    [Required(ErrorMessage = "Please select a round type")]
    [Display(Name = "Round Type")]
    public Guid RoundId { get; set; }
    
    // Additional context for display
    public int PreviousRoundAdvancingTeams { get; set; }
}