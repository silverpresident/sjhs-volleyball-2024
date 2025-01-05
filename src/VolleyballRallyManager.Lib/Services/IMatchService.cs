using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public interface IMatchService
{
    Task<Match?> GetMatchAsync(Guid id);
    Task<List<Match>> GetMatchesAsync();
    Task<List<Match>> GetMatchesByRoundAsync(Guid roundId);
    Task<List<Match>> GetMatchesByTeamAsync(Guid teamId);
    Task<List<Match>> GetInProgressMatchesAsync();
    Task<List<Match>> GetFinishedMatchesAsync();
    Task<List<Match>> GetDisputedMatchesAsync();
    Task<Match> CreateMatchAsync(Match match);
    Task<Match> UpdateMatchAsync(Match match);
    Task<bool> DeleteMatchAsync(Guid id);
    Task<Match> UpdateMatchScoreAsync(Guid id, int homeTeamScore, int awayTeamScore, string userId);
    Task<Match> StartMatchAsync(Guid id, string userId);
    Task<Match> FinishMatchAsync(Guid id, string userId);
    Task<Match> RaiseDisputeAsync(Guid id, string userId);
    Task<Match> AssignRefereeAsync(Guid id, string refereeName);
    Task<Match> AssignScorerAsync(Guid id, string scorerName);
    Task<Match> UpdateLocationAsync(Guid id, string location);
    Task<Match> UpdateTimeAsync(Guid id, DateTime scheduledTime);
    Task<List<MatchUpdate>> GetMatchUpdatesAsync(Guid matchId);
    Task<MatchUpdate> AddMatchUpdateAsync(MatchUpdate update);
    Task<bool> HasTeamPlayedInRoundAsync(Guid teamId, Guid roundId);
    Task<bool> AreTeamsAvailableAsync(Guid homeTeamId, Guid awayTeamId, DateTime scheduledTime);
    Task<bool> IsCourtAvailableAsync(string courtLocation, DateTime scheduledTime);
}
