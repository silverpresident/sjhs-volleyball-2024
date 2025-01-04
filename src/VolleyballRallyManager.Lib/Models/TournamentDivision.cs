using System;

namespace VolleyballRallyManager.Lib.Models
{
    public class TournamentDivision
    {
        public Guid TournamentId { get; set; }
        public Tournament Tournament { get; set; }

        public Guid DivisionId { get; set; }
        public Division Division { get; set; }
    }
}
