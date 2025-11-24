using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VolleyballRallyManager.Lib.Models;

[Index(nameof(MatchId), nameof(SetNumber), IsUnique = true)]
public class MatchSet : BaseEntity
{
    [Display(Name = "Match")]
    public required Guid MatchId { get; set; }

    [Display(Name = "Set Number")]
    public int SetNumber { get; set; } 

    [Display(Name = "Home Team Score")]
    public int HomeTeamScore { get; set; } = 0;

    [Display(Name = "Away Team Score")]
    public int AwayTeamScore { get; set; } = 0;

    [Display(Name = "Is Finished")]
    public bool IsFinished { get; set; } = false;

    [Display(Name = "Is Locked")]
    public bool IsLocked { get; set; } = false;

    // Navigation property
    public virtual Match? Match { get; set; }
}
