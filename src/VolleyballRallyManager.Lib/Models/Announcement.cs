using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VolleyballRallyManager.Lib.Models;

public class Announcement : BaseEntity
{
    [Display(Name = "Title")]
    [StringLength(200)]
    [ValidateNever]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Content")]
    //Content is markdown text
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Priority")]
    public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Routine;

    [Display(Name = "Sequencing Number")]
    [ValidateNever]
    public int SequencingNumber { get; set; }

    [Display(Name = "First Announcement Time")]
    [ValidateNever]
    public DateTime? FirstAnnouncementTime { get; set; }

    [Display(Name = "Last Announcement Time")]
    [ValidateNever]
    public DateTime? LastAnnouncementTime { get; set; }

    [Display(Name = "Remaining Repeat Count")]
    [ValidateNever]
    public int RemainingRepeatCount { get; set; } = 1;

    [Display(Name = "Announced Count")]
    [ValidateNever]
    public int AnnouncedCount { get; set; } = 0;

    [Display(Name = "Is Hidden")]
    [ValidateNever]
    public bool IsHidden { get; set; } = false;

    // Navigation property
    public virtual ICollection<AnnouncementHistoryLog> HistoryLogs { get; set; } = new List<AnnouncementHistoryLog>();
    [ValidateNever]
    public Guid TournamentId { get; set; }
}
