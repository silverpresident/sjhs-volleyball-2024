using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

public class MatchFinalizeViewModel
{
    public Guid MatchId { get; set; }
    
    [Display(Name = "Match Number")]
    public int MatchNumber { get; set; }
    
    [Display(Name = "Division")]
    public string DivisionName { get; set; } = string.Empty;
    
    [Display(Name = "Round")]
    public string RoundName { get; set; } = string.Empty;
    
    [Display(Name = "Court Location")]
    public string CourtLocation { get; set; } = string.Empty;
    
    [Display(Name = "Referee Name")]
    public string? RefereeName { get; set; }
    
    [Display(Name = "Scorer Name")]
    public string? ScorerName { get; set; }
    
    [Display(Name = "Home Team")]
    public string HomeTeamName { get; set; } = string.Empty;
    
    [Display(Name = "Away Team")]
    public string AwayTeamName { get; set; } = string.Empty;
    
    [Display(Name = "Home Team Score")]
    public int HomeTeamScore { get; set; }
    
    [Display(Name = "Away Team Score")]
    public int AwayTeamScore { get; set; }
    
    [Display(Name = "Match Finished")]
    public bool IsFinished { get; set; }
    
    [Display(Name = "Match Disputed")]
    public bool IsDisputed { get; set; }
    
    [Display(Name = "Match Locked")]
    public bool IsLocked { get; set; }
    
    public bool HasSets { get; set; }
}
