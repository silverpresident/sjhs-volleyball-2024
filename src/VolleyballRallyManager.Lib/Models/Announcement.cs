namespace VolleyballRallyManager.Lib.Models;

public class Announcement : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public string RenderedContent { get; set; } = string.Empty;
    public bool UseMarkdown { get; set; }
    public AnnouncementPriority Priority { get; set; }
    public bool IsVisible { get; set; }
}
