using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models;

public class Round : BaseEntity
{
    [Display(Name = "Name")]
    public required string Name { get; set; }

    [Display(Name = "Sequence")]
    public required int Sequence { get; set; }

    // Navigation properties
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
}
