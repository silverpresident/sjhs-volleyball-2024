using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Models;

public class DashboardViewModel
{
    public int TotalTeams { get; set; }
    public int TotalMatches { get; set; }
    public int MatchesInProgress { get; set; }
    public int MatchesFinished { get; set; }
    public int DisputedMatches { get; set; }

    public IEnumerable<Match> RecentMatches { get; set; } = new List<Match>();
    public IEnumerable<Bulletin> RecentBulletins { get; set; } = new List<Bulletin>();
    public Dictionary<string, int> TeamsByDivision { get; set; } = new Dictionary<string, int>();
    public string ActiveTournamentName { get; internal set; } = "ST JAGO VOLLEYBALL RALLY";
    public Tournament? ActiveTournament { get; set; }
}
