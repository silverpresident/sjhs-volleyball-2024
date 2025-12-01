using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Models;

public class TournamentTeamDetailsViewModel
{
    public TournamentTeamDivision TournamentTeamDivision { get; set; } = null!;
    public List<Match> Matches { get; set; } = new List<Match>();
    public Division Division { get; set; } = null!;
}
