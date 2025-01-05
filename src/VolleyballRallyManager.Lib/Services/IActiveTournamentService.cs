using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services
{
    public interface IActiveTournamentService
    {
        Task<Tournament> GetActiveTournamentAsync();
        Task<IEnumerable<TournamentDivision>> GetTournamentDivisionsAsync();
        Task<IEnumerable<TournamentTeamDivision>> GetTournamentTeamsAsync(Guid divisionId);
        Task<TournamentTeamDivision> GetTeamAsync(Guid teamId);
        Task RemoveTeamAsync(Guid teamId);
        Task<TournamentTeamDivision> AddTeamAsync(Guid teamId, Guid divisionId, string groupName, int seedNumber = 0);
        Task<TournamentTeamDivision> SetTeamAsync(Guid teamId, Guid divisionId, string groupName, int seedNumber = 0);
        Task<int> MatchCountAsync(Guid? divisionId = null);
        Task<int> TeamCountAsync(Guid? divisionId = null);

        Task UpdateTeamStatisticsAsync(Match match);

    }
}
