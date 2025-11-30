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
    Task<Match> AssignRefereeAsync(Guid id, string refereeName, string userName);
    Task<Match> AssignScorerAsync(Guid id, string scorerName, string userName);
    Task<Match> UpdateLocationAsync(Guid id, string location, string userName);
    Task<Match> UpdateTimeAsync(Guid id, DateTime scheduledTime, string userName);
    Task<List<MatchUpdate>> GetMatchUpdatesAsync(Guid matchId);
    Task<MatchUpdate> AddMatchUpdateAsync(MatchUpdate update);
    Task<bool> HasTeamPlayedInRoundAsync(Guid teamId, Guid roundId);
    Task<bool> AreTeamsAvailableAsync(Guid homeTeamId, Guid awayTeamId, DateTime scheduledTime);
    Task<bool> IsCourtAvailableAsync(string courtLocation, DateTime scheduledTime);
    
    // MatchSet operations
    Task<List<MatchSet>> GetMatchSetsAsync(Guid matchId);
    Task<MatchSet?> GetCurrentSetAsync(Guid matchId);
    Task<MatchSet> UpdateSetScoreAsync(Guid matchId, int setNumber, int homeScore, int awayScore, string userId);
    Task<MatchSet> FinishSetAsync(Guid matchId, int setNumber, string userId);
    Task<MatchSet> StartNextSetAsync(Guid matchId, string userId, int currentSetNumber);
    Task<MatchSet> RevertToPreviousSetAsync(Guid matchId, string userId, int currentSetNumber);
    Task<Match> EndMatchAndLockSetsAsync(Guid matchId, string userId);
    Task<Match> UpdateMatchDetailsAsync(Guid matchId, DateTime? scheduledTime, string? courtLocation, string? refereeName, string? scorerName, string userId);
    Task<MatchSet?> GetMatchSetAsync(Guid matchId, int currentSetNumber);
    Task<MatchSet> GetOrCreateMatchSetAsync(Guid matchId, int setNumber, string userName);
    Task<bool> IsCalledToCourt(Guid matchId);
}
