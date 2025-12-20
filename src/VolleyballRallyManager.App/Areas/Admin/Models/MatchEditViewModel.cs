using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

public class MatchEditViewModel
{
    public Guid Id { get; set; }

    [Display(Name = "Match Number")]
    public int MatchNumber { get; set; }

    [Required]
    [Display(Name = "Round")]
    public Guid RoundId { get; set; }

    [Required]
    [Display(Name = "Scheduled Time")]
    public DateTime ScheduledTime { get; set; }

    [Display(Name = "Actual Start Time")]
    public DateTime? ActualStartTime { get; set; }

    [Required]
    [Display(Name = "Court Location")]
    public string CourtLocation { get; set; } = "Unassigned";

    [Required]
    [Display(Name = "Home Team")]
    public Guid HomeTeamId { get; set; }

    [Required]
    [Display(Name = "Away Team")]
    public Guid AwayTeamId { get; set; }

    [Display(Name = "Referee Name")]
    public string? RefereeName { get; set; }

    [Display(Name = "Scorer Name")]
    public string? ScorerName { get; set; }

    [Display(Name = "Home Team Score")]
    public int HomeTeamScore { get; set; }

    [Display(Name = "Away Team Score")]
    public int AwayTeamScore { get; set; }

    [Display(Name = "Is Finished")]
    public bool IsFinished { get; set; }

    [Display(Name = "Is Disputed")]
    public bool IsDisputed { get; set; }

    [Display(Name = "Is Locked")]
    public bool IsLocked { get; set; }

    // For display and dropdowns
    public string? DivisionName { get; set; }
    public string? HomeTeamName { get; set; }
    public string? AwayTeamName { get; set; }
    
    public SelectList? Teams { get; set; }
    public SelectList? Rounds { get; set; }

    public List<MatchUpdate> Updates { get; set; } = new List<MatchUpdate>();
}
