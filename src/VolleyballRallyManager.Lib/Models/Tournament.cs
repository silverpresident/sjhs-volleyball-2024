using System;

namespace VolleyballRallyManager.Lib.Models
{
    public class Tournament : BaseEntity
    {
        public string Name { get; set; }= string.Empty;
        public string Description { get; set; }= string.Empty;
        public DateTime TournamentDate { get; set; }
        public bool IsActive { get; set; } = false;
        public ICollection<TournamentTeamDivision> TournamentTeamDivisions { get; set; }
    }
}
