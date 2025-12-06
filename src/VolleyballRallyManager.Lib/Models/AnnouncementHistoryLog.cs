using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VolleyballRallyManager.Lib.Models;

public class AnnouncementHistoryLog : BaseEntity
{
    [Required]
    [Display(Name = "Announcement")]
    public Guid AnnouncementId { get; set; }

    [Display(Name = "Timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.Now;

    // Navigation property
    [ForeignKey("AnnouncementId")]
    public virtual Announcement? Announcement { get; set; }
}
