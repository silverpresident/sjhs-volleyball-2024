using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models;

public class Announcement : BaseEntity
{
    [Display(Name = "Content")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Rendered Content")]
    public string RenderedContent { get; set; } = string.Empty;

    [Display(Name = "Use Markdown")]
    public bool UseMarkdown { get; set; }

    [Display(Name = "Priority")]
    public AnnouncementPriority Priority { get; set; }

    [Display(Name = "Is Visible")]
    public bool IsVisible { get; set; }
}
