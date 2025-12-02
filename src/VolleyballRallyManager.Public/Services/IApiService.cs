using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Public.Services;

public interface IApiService
{
    Task<List<Match>> GetMatchesAsync();
    Task<List<Team>> GetTeamsAsync();
    Task<List<Announcement>> GetAnnouncementsAsync();
    Task<List<MatchUpdate>> GetMatchUpdatesAsync();
    Task<List<Division>> GetDivisionsAsync();
    Task<Tournament?> GetActiveTournamentAsync();
}
