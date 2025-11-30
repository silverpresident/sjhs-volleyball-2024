using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models;

public enum BulletinPriority
{
    [Display(Name = "Info")]
    Info,
    [Display(Name = "Warning")]
    Warning,
    [Display(Name = "Danger")]
    Danger,
    [Display(Name = "Primary")]
    Primary,
    [Display(Name = "Secondary")]
    Secondary
}
