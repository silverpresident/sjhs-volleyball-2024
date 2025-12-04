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
        public List<TournamentRoundViewModel> Rounds { get; set; } = new List<TournamentRoundViewModel>();
        public List<TournamentTeamDivision> Teams { get; set; } = new List<TournamentTeamDivision>();
        public Dictionary<Division, List<TournamentTeamDivision>> TeamsByDivision = new();
    }

    /// <summary>
    /// ViewModel for division statistics within a tournament
    /// </summary>
    public class TournamentDivisionViewModel
    {
        public Guid DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public List<string> GroupNames { get; set; } = new List<string>(); 
        public int RoundsCount { get; set; } 
        public int MatchCount { get; set; }
       
        public int TeamCount { get; set; }
        public int MatchesPlayed { get; set; }
        public Division Division { get; set; }
    }
}
