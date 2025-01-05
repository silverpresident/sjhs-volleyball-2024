using System.Collections.Generic;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.App.Models
{
    public class ActiveTournamentViewModel
    {
        public Tournament ActiveTournament { get; set; }
        public IEnumerable<TournamentDivision> Divisions { get; set; }
        public Dictionary<TournamentDivision, IEnumerable<TournamentTeamDivision>> TeamsByDivision { get; set; }
    }
}
