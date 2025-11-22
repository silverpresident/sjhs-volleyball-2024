namespace VolleyballRallyManager.Lib.Models
{
    public class TournamentDivision
    {
        public Guid TournamentId { get; set; }
        public required Tournament Tournament { get; set; }

        public Guid DivisionId { get; set; }
        public required Division Division { get; set; }
    }
}
