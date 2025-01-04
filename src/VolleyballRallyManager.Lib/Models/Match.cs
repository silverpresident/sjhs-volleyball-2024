namespace VolleyballRallyManager.Lib.Models;

public class Match : BaseEntity
{
    public int MatchNumber { get; set; }
    public required Guid RoundId { get; set; }
    public required DateTime ScheduledTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public required string CourtLocation { get; set; }
    public Guid TournamentId { get; set; }
    public Tournament Tournament { get; set; }
    
    public required Guid HomeTeamId { get; set; }
    public required Guid AwayTeamId { get; set; }
    
    public int HomeTeamScore { get; set; }
    public int AwayTeamScore { get; set; }
    
    public bool IsFinished { get; set; }
    public bool IsDisputed { get; set; }
    
    public string? RefereeName { get; set; }
    public string? ScorerName { get; set; }

    // Navigation properties
    public virtual Round? Round { get; set; }
    public virtual Team? HomeTeam { get; set; }
    public virtual Team? AwayTeam { get; set; }
    public virtual ICollection<MatchUpdate> Updates { get; set; } = new List<MatchUpdate>();
}
