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
    public int HomeTeamScore { get; set; }

    [Display(Name = "Away Team Score")]
    public int AwayTeamScore { get; set; }

    [Display(Name = "Is Finished")]
    public bool IsFinished { get; set; }

    [Display(Name = "Is Locked")]
    public bool IsLocked { get; set; }

    // Navigation property
    public virtual Match? Match { get; set; }
}
