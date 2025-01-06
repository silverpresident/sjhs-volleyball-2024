using System.Collections.Generic;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Models
{
    public class ActiveTournamentViewModel
    {
        public Tournament ActiveTournament { get; set; }
        public List<Division> Divisions { get; set; } = new List<Division>();
        public IEnumerable<TournamentDivision> TournamentDivisions { get; set; } = new List<TournamentDivision>();
        public Dictionary<TournamentDivision, IEnumerable<TournamentTeamDivision>> TeamsByDivision { get; set; }
    }
}
