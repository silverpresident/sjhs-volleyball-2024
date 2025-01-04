using System;

namespace VolleyballRallyManager.Lib.Models
{
    public class Tournament : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime TournamentDate { get; set; }
        public bool IsActive { get; set; }
        public ICollection<TournamentTeamDivision> TournamentTeamDivisions { get; set; }
    }
}
