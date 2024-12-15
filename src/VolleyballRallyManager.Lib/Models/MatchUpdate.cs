namespace VolleyballRallyManager.Lib.Models;

public class MatchUpdate : BaseEntity
{
    public required Guid MatchId { get; set; }
    public required string UpdateText { get; set; }
    public required UpdateType UpdateType { get; set; }
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Navigation property
    public virtual Match? Match { get; set; }
}
