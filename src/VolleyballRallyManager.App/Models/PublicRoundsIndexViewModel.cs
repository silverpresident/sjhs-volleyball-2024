using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Models;

/// <summary>
/// ViewModel for the public RoundTemplates index page
/// </summary>
public class PublicRoundsIndexViewModel
{
    public string TournamentName { get; set; } = string.Empty;
    public Guid TournamentId { get; set; }
    public List<DivisionRoundsGroup> DivisionGroups { get; set; } = new List<DivisionRoundsGroup>();
}

/// <summary>
/// Groups rounds by division for display
/// </summary>
public class DivisionRoundsGroup
{
    public Guid DivisionId { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public List<TournamentRoundSummaryViewModel> Rounds { get; set; } = new List<TournamentRoundSummaryViewModel>();
}
