using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models;

public class RoundTemplate : BaseEntity
{
    [Display(Name = "Name")]
    public required string Name { get; set; }

    [Display(Name = "Sequence")]
    public required int Sequence { get; set; }

    [Display(Name = "Recommended Qualifying Teams Count")]
    public int RecommendedQualifyingTeamsCount { get; set; } = 0;

    [Required]
    [Display(Name = "Recommended Match Generation Strategy")]
    public MatchGenerationStrategy RecommendedMatchGenerationStrategy { get; set; } = MatchGenerationStrategy.RoundRobin;

    [Required]
    [Display(Name = "Recommended Team Selection Strategy")]
    public TeamSelectionStrategy RecommendedTeamSelectionStrategy { get; set; } = TeamSelectionStrategy.TopByPoints;

    [Display(Name = "Is Playoff Round")]
    public bool IsPlayoff { get; set; } = false;

    // Navigation properties
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
}
