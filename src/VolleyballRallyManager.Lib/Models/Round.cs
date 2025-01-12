using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models;

public class Round : BaseEntity
{
    [Display(Name = "Name")]
    public required string Name { get; set; }

    [Display(Name = "Sequence")]
    public required int Sequence { get; set; }

    [Display(Name = "Qualifying Teams")]
    public required int QualifyingTeams { get; set; } = 0;

    // Navigation properties
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
}
