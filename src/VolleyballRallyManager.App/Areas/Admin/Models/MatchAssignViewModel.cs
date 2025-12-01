using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

public class MatchAssignViewModel
{
    public Guid MatchId { get; set; }
    
    [Display(Name = "Match Number")]
    public int MatchNumber { get; set; }
    
    [Display(Name = "Division")]
    public string DivisionName { get; set; } = string.Empty;
    
    [Display(Name = "Round")]
    public string RoundName { get; set; } = string.Empty;
    
    [Display(Name = "Home Team")]
    public string HomeTeamName { get; set; } = string.Empty;
    
    [Display(Name = "Away Team")]
    public string AwayTeamName { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Scheduled Time")]
    public DateTime ScheduledTime { get; set; }
    
    [Required]
    [Display(Name = "Court Location")]
    public string CourtLocation { get; set; } = string.Empty;
    
    [Display(Name = "Referee Name")]
    public string? RefereeName { get; set; }
    
    [Display(Name = "Scorer Name")]
    public string? ScorerName { get; set; }
    
    public List<string> AvailableCourts { get; set; } = new List<string>
    {
        "Court 1",
        "Court 2",
        "Court 3",
        "Court 4",
        "Court 5",
        "Court 6",
        "Court 7",
        "Court 8",
        "Court 9"
    };
}
