using System.ComponentModel.DataAnnotations;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

public class AutoSeedViewModel
{
    public Tournament? ActiveTournament { get; set; }
    public List<Division> AvailableDivisions { get; set; } = new List<Division>();
    
    [Display(Name = "Division")]
    public Guid? SelectedDivisionId { get; set; }
    
    [Required]
    [Display(Name = "Seeding Method")]
    public SeedingMethod SeedingMethod { get; set; } = SeedingMethod.SeedUnseeded;
    
    [Required]
    [Display(Name = "Sorting Method")]
    public SortingMethod SortingMethod { get; set; } = SortingMethod.ByCreationDate;
    
    [Display(Name = "Seed Placement")]
    public SeedPlacement SeedPlacement { get; set; } = SeedPlacement.FillGaps;
    
    [Required]
    [Display(Name = "Seed Gap Closure")]
    public SeedGapClosure GapClosure { get; set; } = SeedGapClosure.LetGapsRemain;
}

public enum SeedingMethod
{
    [Display(Name = "Seed Unseeded Teams")]
    SeedUnseeded,
    
    [Display(Name = "Reseed All Teams")]
    ReseedAll
}

public enum SortingMethod
{
    [Display(Name = "By Creation Date")]
    ByCreationDate,

    [Display(Name = "By Team Name")]
    ByTeamName,

    [Display(Name = "By Rating")]
    ByRating,

    [Display(Name = "Randomly")]
    Randomly
}


public enum SeedPlacement
{
    [Display(Name = "Fill Gaps")]
    FillGaps,
    
    [Display(Name = "At the End")]
    AtTheEnd
}

public enum SeedGapClosure
{
    [Display(Name = "Let Gaps Remain")]
    LetGapsRemain,
    
    [Display(Name = "Close All Gaps")]
    CloseAllGaps
}
