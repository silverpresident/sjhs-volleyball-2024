using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models;

public class Announcement : BaseEntity
{
    [Required]
    [Display(Name = "Title")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Content")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Priority")]
    public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Info;

    [Display(Name = "Sequencing Number")]
    public int SequencingNumber { get; set; }

    [Display(Name = "First Announcement Time")]
    public DateTime? FirstAnnouncementTime { get; set; }

    [Display(Name = "Last Announcement Time")]
    public DateTime? LastAnnouncementTime { get; set; }

    [Display(Name = "Remaining Repeat Count")]
    public int RemainingRepeatCount { get; set; } = 1;

    [Display(Name = "Announced Count")]
    public int AnnouncedCount { get; set; } = 0;

    [Display(Name = "Is Hidden")]
    public bool IsHidden { get; set; } = false;

    // Navigation property
    public virtual ICollection<AnnouncementHistoryLog> HistoryLogs { get; set; } = new List<AnnouncementHistoryLog>();
}
