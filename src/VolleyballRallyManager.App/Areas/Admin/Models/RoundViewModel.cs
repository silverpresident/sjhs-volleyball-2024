using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

public class RoundViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public int QualifyingTeams { get; set; }
    public int TotalMatches { get; set; }
    public int CompletedMatches { get; set; }
    public int PendingMatches { get; set; }
    public double CompletionPercentage { get; set; }
    public bool IsComplete { get; set; }
}

public class RoundsIndexViewModel
{
    public IEnumerable<RoundViewModel> Rounds { get; set; } = new List<RoundViewModel>();
    public int TotalRounds { get; set; }
    public int TotalMatches { get; set; }
    public int TotalCompletedMatches { get; set; }
    public int TotalPendingMatches { get; set; }
}
