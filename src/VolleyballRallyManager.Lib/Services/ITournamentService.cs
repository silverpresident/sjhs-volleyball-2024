using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services
{
    public interface ITournamentService
    {
        Task SetActiveTournamentAsync(Guid tournamentId);
        Task<IEnumerable<Division>> GetTournamentDivisionsAsync(Guid tournamentId);
        Task<IEnumerable<Team>> GetTournamentTeamsAsync(Guid tournamentId);
        Task<IEnumerable<Match>> GetTournamentMatchesAsync(Guid tournamentId);
        Task AddDivisionToTournamentAsync(Guid tournamentId, Guid divisionId);
        Task RemoveDivisionFromTournamentAsync(Guid tournamentId, Guid divisionId);
        Task AddTeamToTournamentAsync(Guid tournamentId, Guid teamId, Guid divisionId, string group, int seedNumber);
        Task RemoveTeamFromTournamentAsync(Guid tournamentId, Guid teamId);
        Task<Tournament?> GetActiveTournamentAsync();
        Task<Tournament?> GetTournamentByIdAsync(Guid tournamentId);
        Task<IEnumerable<Tournament>> GetAllTournamentsAsync();
        Task UpdateTournamentAsync(Tournament tournament);
        Task<IEnumerable<Team>> GetTeamsByDivisionAsync(Division division);
        Task<IEnumerable<Team>> GetLeaderboardAsync(Guid divisionId);
        Task RecalculateTeamStatisticsAsync(Guid teamId);
        Task GetDetailsAsync(Tournament tournament);
    }
}
