using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models;

public class Bulletin : BaseEntity
{
    [Display(Name = "Content")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Rendered Content")]
    public string RenderedContent { get; set; } = string.Empty;

    [Display(Name = "Use Markdown")]
    public bool UseMarkdown { get; set; } = true;

    [Display(Name = "Priority")]
    public BulletinPriority Priority { get; set; } = BulletinPriority.Info;


    [Display(Name = "Is Visible")]
    public bool IsVisible { get; set; } = true;
}
