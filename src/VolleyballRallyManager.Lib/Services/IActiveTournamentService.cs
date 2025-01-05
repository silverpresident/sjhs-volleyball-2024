using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services
{
    public interface IActiveTournamentService
    {
        Task<Tournament> GetActiveTournamentAsync();
        Task<IEnumerable<TournamentDivision>> GetTournamentDivisionsAsync(Guid tournamentId);
        Task<IEnumerable<TournamentTeamDivision>> GetTournamentTeamsAsync(Guid tournamentId);
    }
}
