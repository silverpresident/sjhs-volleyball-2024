using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models;

public enum AnnouncementPriority
{
    [Display(Name = "Urgent")]
    Urgent,
    
    [Display(Name = "Info")]
    Info,
    
    [Display(Name = "Routine")]
    Routine
}
