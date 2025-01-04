using System;
using System.Collections.Generic;

namespace VolleyballRallyManager.Lib.Models
{
    public class Division : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
        public virtual ICollection<TournamentTeamDivision> TournamentTeamDivisions { get; set; } = new List<TournamentTeamDivision>();
    }
}
