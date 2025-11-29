using VolleyballRallyManager.Lib.Common;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services
{
    public interface IActiveTournamentService
    {
        Task<Tournament?> GetActiveTournamentAsync();
        Task<IEnumerable<TournamentDivision>> GetTournamentDivisionsAsync();
        Task<IEnumerable<TournamentTeamDivision>> GetTournamentTeamsAsync(Guid divisionId);
        Task<int> GetTournamentTeamsCountAsync(Guid divisionId);
        Task<TournamentTeamDivision?> GetTeamAsync(Guid teamId);
        Task RemoveTeamAsync(Guid teamId);
        Task<TournamentTeamDivision> AddTeamAsync(Guid teamId, Guid divisionId, string groupName, int seedNumber = 0);
        Task<TournamentTeamDivision> SetTeamAsync(Guid teamId, Guid divisionId, string groupName, int seedNumber = 0);
        Task<int> MatchCountAsync(MatchState matchState = MatchState.None, Guid? divisionId = null);
        Task<int> TeamCountAsync(Guid? divisionId = null);

        Task UpdateTeamStatisticsAsync(Match match);
        Task<IEnumerable<Division>> GetAvailableDivisionsAsync();
        Task<IEnumerable<Team>> GetAvailableTeamsAsync();
        Task<IEnumerable<Match>> GetMatchesAsync(Guid? divisionId = null, Guid? roundId = null, string? groupName = null, Guid? teamId = null);
        Task UpdateTournamentDivisionsAsync(List<Guid> selectedDivisionIds);
        Task<IEnumerable<Match>> RecentMatchesAsync();
    }
}
