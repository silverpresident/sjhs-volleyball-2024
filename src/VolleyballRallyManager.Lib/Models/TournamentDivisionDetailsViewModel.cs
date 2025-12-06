namespace VolleyballRallyManager.Lib.Models
{
    /// <summary>
    /// ViewModel for displaying details for a single division within a tournament, including all rounds
    /// </summary>
    public class TournamentDivisionDetailsViewModel
    {
        public Guid TournamentId { get; set; }
        public Guid DivisionId { get; set; }
        public Tournament Tournament { get; set; } = null!;
        public Division Division { get; set; } = null!;
        public TournamentDivisionViewModel DivisionStats { get; set; } = null!;
        public List<TournamentRoundViewModel> Rounds { get; set; } = new List<TournamentRoundViewModel>();
        public List<TournamentTeamDivision> Teams { get; set; } = new List<TournamentTeamDivision>();
    }
}
