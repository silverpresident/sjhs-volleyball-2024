namespace VolleyballRallyManager.Lib.Models;

public class Round : BaseEntity
{
    public required string Name { get; set; }
    public required int Sequence { get; set; }
    public bool IsComplete { get; set; }

    // Navigation properties
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
}
