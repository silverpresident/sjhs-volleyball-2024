using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models;

public class MatchUpdate : BaseEntity
{
    [Display(Name = "Match")]
    public required Guid MatchId { get; set; }

    [Display(Name = "Content")]
    //Content is markdown text
    public required string Content { get; set; }

    [Display(Name = "Update Type")]
    public required UpdateType UpdateType { get; set; }

    [Display(Name = "Previous Value")]
    public string? PreviousValue { get; set; }

    [Display(Name = "New Value")]
    public string? NewValue { get; set; }

    [Display(Name = "Is Processed")]
    public bool IsProcessed { get; set; }

    [Display(Name = "Processed At")]
    public DateTime? ProcessedAt { get; set; }

    // Navigation property
    public virtual Match? Match { get; set; }
}
