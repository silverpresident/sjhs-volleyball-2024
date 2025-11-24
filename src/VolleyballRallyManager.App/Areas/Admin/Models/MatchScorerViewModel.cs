using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

public class MatchScorerViewModel
{
    public Guid MatchId { get; set; }
    public int MatchNumber { get; set; }
    public string HomeTeamName { get; set; } = string.Empty;
    public Guid HomeTeamId { get; set; }
    public string AwayTeamName { get; set; } = string.Empty;
    public Guid AwayTeamId { get; set; }
    public string CourtLocation { get; set; } = string.Empty;
    public int CurrentSetNumber { get; set; }
    public bool IsFinished { get; set; }
    public bool IsDisputed { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public List<MatchSetDto> Sets { get; set; } = new List<MatchSetDto>();
    public List<MatchUpdate> RecentUpdates { get; set; } = new List<MatchUpdate>();
}

public class MatchSetDto
{
    public int SetNumber { get; set; }
    public int HomeTeamScore { get; set; }
    public int AwayTeamScore { get; set; }
    public bool IsFinished { get; set; }
    public bool IsLocked { get; set; }
}
