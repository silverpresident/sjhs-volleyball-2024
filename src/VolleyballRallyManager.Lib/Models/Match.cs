using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using VolleyballRallyManager.Lib.Common;

namespace VolleyballRallyManager.Lib.Models;

public class Match : BaseEntity
{
    [Display(Name = "Match Number")]
    public int MatchNumber { get; set; }
    [Display(Name = "Round")]
    public required Guid TournamentRoundId { get; set; }

    [Display(Name = "Round")]
    public required Guid RoundTemplateId { get; set; }

    [Display(Name = "Scheduled Time")]
    public required DateTime ScheduledTime { get; set; }

    [Display(Name = "Actual Start Time")]
    public DateTime? ActualStartTime { get; set; }

    [Display(Name = "Court Location")]
    public required string CourtLocation { get; set; } = "Unassigned";

    [Display(Name = "Group")]
    public required string GroupName { get; set; } = string.Empty;

    [Display(Name = "Tournament")]
    public Guid TournamentId { get; set; }
    [Display(Name = "Division")]
    public Guid DivisionId { get; set; }

    [Display(Name = "Tournament")]
    public Tournament Tournament { get; set; } = null!;

    [Display(Name = "Home Team")]
    public required Guid HomeTeamId { get; set; }

    [Display(Name = "Away Team")]
    public required Guid AwayTeamId { get; set; }

    [Display(Name = "Home Team Score")]
    public int HomeTeamScore { get; set; } = 0;

    [Display(Name = "Away Team Score")]
    public int AwayTeamScore { get; set; } = 0;

    [Display(Name = "Is Finished")]
    public bool IsFinished { get; set; } = false;

    [Display(Name = "Is Disputed")]
    public bool IsDisputed { get; set; } = false;

    [Display(Name = "Referee Name")]
    public string? RefereeName { get; set; }

    [Display(Name = "Scorer Name")]
    public string? ScorerName { get; set; }

    [Display(Name = "Current Set Number")]
    public int CurrentSetNumber { get; set; } = 0;

    [Display(Name = "Is Locked")]
    public bool IsLocked { get; set; } = false;

    // Navigation properties
    public virtual Division? Division { get; set; }
    public virtual RoundTemplate? Round { get; set; }
    public virtual Team? HomeTeam { get; set; }
    public virtual Team? AwayTeam { get; set; }
    public virtual ICollection<MatchUpdate> Updates { get; set; } = new List<MatchUpdate>();
    public virtual ICollection<MatchSet> Sets { get; set; } = new List<MatchSet>();


    public virtual MatchState MatchState
    {
        get
        {
            if (IsDisputed)
            {
                return MatchState.Disputed;
            }
            if (IsFinished)
            {
                return MatchState.Finished;
            }
            if (ActualStartTime.HasValue)
            {
                return MatchState.InProgress;
            }
            if (ScheduledTime <= DateTime.Now)
            {
                return MatchState.Scheduled;
            }
            return MatchState.None;

        }
    }
}
