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
    public IEnumerable<Announcement> RecentAnnouncements { get; set; } = new List<Announcement>();
    public Dictionary<string, int> TeamsByDivision { get; set; } = new Dictionary<string, int>();
}
