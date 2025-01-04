using System.Collections.Generic;
using System.Threading.Tasks;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services
{
    public interface ITournamentService
    {
        Task SetActiveTournament(Guid tournamentId);
        Task<IEnumerable<Division>> GetTournamentDivisions(Guid tournamentId);
        Task<IEnumerable<Team>> GetTournamentTeams(Guid tournamentId);
        Task<IEnumerable<Match>> GetTournamentMatches(Guid tournamentId);
        Task AddDivisionToTournament(Guid tournamentId, Guid divisionId);
        Task RemoveDivisionFromTournament(Guid tournamentId, Guid divisionId);
        Task AddTeamToTournament(Guid tournamentId, Guid teamId, Guid divisionId, string group, int seedNumber);
        Task RemoveTeamFromTournament(Guid tournamentId, Guid teamId);
    }
}
