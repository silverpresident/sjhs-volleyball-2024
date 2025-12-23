namespace VolleyballRallyManager.Lib.Models;


public class TournamentTeamDetailsViewModel
{
    public TournamentTeamDivision Team { get; set; } = null!;
    public List<Match> Matches { get; set; } = new List<Match>();
    public Division Division { get; set; } = null!;
}
