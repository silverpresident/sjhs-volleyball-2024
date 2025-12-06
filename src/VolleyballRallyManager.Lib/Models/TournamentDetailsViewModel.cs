using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Models
{
    /// <summary>
    /// ViewModel for displaying comprehensive tournament details including divisions, rounds, and teams
    /// </summary>
    public class TournamentDetailsViewModel
    {
        public Guid TournamentId { get; set; }
        public Tournament Tournament { get; set; } = null!;
        public List<TournamentDivisionViewModel> Divisions { get; set; } = new List<TournamentDivisionViewModel>();
        public List<TournamentRoundSummaryViewModel> Rounds { get; set; } = new List<TournamentRoundSummaryViewModel>();
        public List<TournamentTeamDivision> Teams { get; set; } = new List<TournamentTeamDivision>();
        public Dictionary<Division, List<TournamentTeamDivision>> TeamsByDivision = new();
    }
}
