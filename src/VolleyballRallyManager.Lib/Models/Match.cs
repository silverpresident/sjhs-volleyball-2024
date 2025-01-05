using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models;

public class Match : BaseEntity
{
    [Display(Name = "Match Number")]
    public int MatchNumber { get; set; }

    [Display(Name = "Round")]
    public required Guid RoundId { get; set; }

    [Display(Name = "Scheduled Time")]
    public required DateTime ScheduledTime { get; set; }

    [Display(Name = "Actual Start Time")]
    public DateTime? ActualStartTime { get; set; }

    [Display(Name = "Court Location")]
    public required string CourtLocation { get; set; }

    [Display(Name = "Tournament")]
    public Guid TournamentId { get; set; }

    public Tournament Tournament { get; set; }
    
    [Display(Name = "Home Team")]
    public required Guid HomeTeamId { get; set; }

    [Display(Name = "Away Team")]
    public required Guid AwayTeamId { get; set; }
    
    [Display(Name = "Home Team Score")]
    public int HomeTeamScore { get; set; }

    [Display(Name = "Away Team Score")]
    public int AwayTeamScore { get; set; }
    
    [Display(Name = "Is Finished")]
    public bool IsFinished { get; set; }

    [Display(Name = "Is Disputed")]
    public bool IsDisputed { get; set; }
    
    [Display(Name = "Referee Name")]
    public string? RefereeName { get; set; }

    [Display(Name = "Scorer Name")]
    public string? ScorerName { get; set; }

    // Navigation properties
    public virtual Round? Round { get; set; }
    public virtual Team? HomeTeam { get; set; }
    public virtual Team? AwayTeam { get; set; }
    public virtual ICollection<MatchUpdate> Updates { get; set; } = new List<MatchUpdate>();
}
