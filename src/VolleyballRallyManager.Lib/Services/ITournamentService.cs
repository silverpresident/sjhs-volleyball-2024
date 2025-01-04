using System.Collections.Generic;
using System.Threading.Tasks;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services
{
    public interface ITournamentService
    {
        Task SetActiveTournament(int tournamentId);
        Task<IEnumerable<Division>> GetTournamentDivisions(Guid tournamentId);
        Task<IEnumerable<Team>> GetTournamentTeams(int tournamentId);
        Task<IEnumerable<Match>> GetTournamentMatches(int tournamentId);
        Task AddDivisionToTournament(Guid tournamentId, Guid divisionId);
        Task RemoveDivisionFromTournament(Guid tournamentId, Guid divisionId);
    }
}
