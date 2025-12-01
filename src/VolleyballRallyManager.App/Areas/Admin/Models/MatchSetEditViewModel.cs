using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

public class MatchSetEditViewModel
{
    public Guid MatchId { get; set; }
    
    [Display(Name = "Set Number")]
    public int SetNumber { get; set; }
    
    [Display(Name = "Home Team Score")]
    [Range(0, 100)]
    public int HomeTeamScore { get; set; }
    
    [Display(Name = "Away Team Score")]
    [Range(0, 100)]
    public int AwayTeamScore { get; set; }
    
    [Display(Name = "Set Finished")]
    public bool IsFinished { get; set; }
    
    [Display(Name = "Set Locked")]
    public bool IsLocked { get; set; }
    
    [Display(Name = "Home Team")]
    public string HomeTeamName { get; set; } = string.Empty;
    
    [Display(Name = "Away Team")]
    public string AwayTeamName { get; set; } = string.Empty;
}
