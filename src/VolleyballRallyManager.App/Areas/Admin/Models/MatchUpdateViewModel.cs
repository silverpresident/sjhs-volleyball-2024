using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

public class MatchUpdateViewModel
{
    public Guid MatchId { get; set; }
    
    [Display(Name = "Match Number")]
    public int MatchNumber { get; set; }
    
    [Display(Name = "Home Team")]
    public string HomeTeamName { get; set; } = string.Empty;
    
    [Display(Name = "Away Team")]
    public string AwayTeamName { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Update Comment")]
    [DataType(DataType.MultilineText)]
    public string Comment { get; set; } = string.Empty;
}
