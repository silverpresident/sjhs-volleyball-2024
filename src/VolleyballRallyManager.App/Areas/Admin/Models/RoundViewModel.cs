using System.ComponentModel.DataAnnotations;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

public class RoundViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; }
    
    // Template recommendation properties
    public int RecommendedQualifyingTeamsCount { get; set; }
    public MatchGenerationStrategy RecommendedMatchGenerationStrategy { get; set; }
    public TeamSelectionStrategy RecommendedTeamSelectionStrategy { get; set; }
    public bool IsPlayoff { get; set; }
}

public class RoundsIndexViewModel
{
    public IEnumerable<RoundViewModel> Rounds { get; set; } = new List<RoundViewModel>();
    public int TotalRounds { get; set; }
}

/// <summary>
/// ViewModel for creating and editing round templates
/// </summary>
public class CreateEditRoundViewModel
{
    public Guid? Id { get; set; } // Null for Create, has value for Edit
    
    [Required]
    [Display(Name = "Round Name")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Sequence")]
    [Range(1, 100, ErrorMessage = "Sequence must be between 1 and 100")]
    public int Sequence { get; set; } = 1;
    
    [Display(Name = "Recommended Qualifying Teams Count")]
    [Range(0, 100, ErrorMessage = "Must be between 0 and 100")]
    public int RecommendedQualifyingTeamsCount { get; set; } = 0;
    
    [Required]
    [Display(Name = "Recommended Match Generation Strategy")]
    public MatchGenerationStrategy RecommendedMatchGenerationStrategy { get; set; } = MatchGenerationStrategy.RoundRobin;
    
    [Required]
    [Display(Name = "Recommended Team Selection Strategy")]
    public TeamSelectionStrategy RecommendedTeamSelectionStrategy { get; set; } = TeamSelectionStrategy.TopByPoints;
    
    [Display(Name = "Is Playoff Round")]
    public bool IsPlayoff { get; set; } = false;
}
