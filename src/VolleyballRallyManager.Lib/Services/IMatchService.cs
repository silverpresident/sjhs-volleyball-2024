using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

/// <summary>
/// Service for managing volleyball matches including creation, updates, scoring, and match sets.
/// Handles match lifecycle from creation through completion, including real-time updates and referee/scorer assignments.
/// </summary>
public interface IMatchService
{
    /// <summary>
    /// Retrieves a specific match by ID.
    /// </summary>
    /// <param name="id">The match ID to retrieve.</param>
    /// <returns>The match or null if not found.</returns>
    Task<Match?> GetMatchAsync(Guid id);

    /// <summary>
    /// Gets all matches in the system.
    /// </summary>
    /// <returns>List of all matches.</returns>
    Task<List<Match>> GetMatchesAsync();

    /// <summary>
    /// Gets all matches for a specific round.
    /// </summary>
    /// <param name="roundId">The round ID to filter by.</param>
    /// <returns>List of matches in the specified round.</returns>
    Task<List<Match>> GetMatchesByRoundAsync(Guid roundId);

    /// <summary>
    /// Gets all matches involving a specific team.
    /// </summary>
    /// <param name="teamId">The team ID to filter by.</param>
    /// <returns>List of matches where the team participates.</returns>
    Task<List<Match>> GetMatchesByTeamAsync(Guid teamId);

    /// <summary>
    /// Gets all matches currently in progress.
    /// </summary>
    /// <returns>List of in-progress matches.</returns>
    Task<List<Match>> GetInProgressMatchesAsync();

    /// <summary>
    /// Gets all completed matches.
    /// </summary>
    /// <returns>List of finished matches.</returns>
    Task<List<Match>> GetFinishedMatchesAsync();

    /// <summary>
    /// Gets all matches that have a dispute raised.
    /// </summary>
    /// <returns>List of disputed matches.</returns>
    Task<List<Match>> GetDisputedMatchesAsync();

    /// <summary>
    /// Creates a new match.
    /// </summary>
    /// <param name="match">The match entity to create.</param>
    /// <returns>The created match.</returns>
    Task<Match> CreateMatchAsync(Match match);

    /// <summary>
    /// Updates an existing match.
    /// </summary>
    /// <param name="match">The match entity with updated values.</param>
    /// <returns>The updated match.</returns>
    Task<Match> UpdateMatchAsync(Match match);

    /// <summary>
    /// Deletes a match by ID.
    /// </summary>
    /// <param name="id">The match ID to delete.</param>
    /// <returns>True if deleted successfully, false otherwise.</returns>
    Task<bool> DeleteMatchAsync(Guid id);

    /// <summary>
    /// Updates the score for a match (legacy method for match-level scoring).
    /// </summary>
    /// <param name="id">The match ID.</param>
    /// <param name="homeTeamScore">New home team score.</param>
    /// <param name="awayTeamScore">New away team score.</param>
    /// <param name="userId">User making the update.</param>
    /// <returns>The updated match.</returns>
    Task<Match> UpdateMatchScoreAsync(Guid id, int homeTeamScore, int awayTeamScore, string userId);

    /// <summary>
    /// Marks a match as started and in progress.
    /// </summary>
    /// <param name="id">The match ID to start.</param>
    /// <param name="userId">User starting the match.</param>
    /// <returns>The updated match.</returns>
    Task<Match> StartMatchAsync(Guid id, string userId);

    /// <summary>
    /// Marks a match as finished and calculates final scores.
    /// </summary>
    /// <param name="id">The match ID to finish.</param>
    /// <param name="userId">User finishing the match.</param>
    /// <returns>The updated match.</returns>
    Task<Match> FinishMatchAsync(Guid id, string userId);

    /// <summary>
    /// Raises a dispute for a match.
    /// </summary>
    /// <param name="id">The match ID to dispute.</param>
    /// <param name="userId">User raising the dispute.</param>
    /// <returns>The updated match with dispute flag.</returns>
    Task<Match> RaiseDisputeAsync(Guid id, string userId);

    /// <summary>
    /// Assigns a referee to a match.
    /// </summary>
    /// <param name="id">The match ID.</param>
    /// <param name="refereeName">Name of the referee.</param>
    /// <param name="userName">User making the assignment.</param>
    /// <returns>The updated match.</returns>
    Task<Match> AssignRefereeAsync(Guid id, string refereeName, string userName);

    /// <summary>
    /// Assigns a scorer to a match.
    /// </summary>
    /// <param name="id">The match ID.</param>
    /// <param name="scorerName">Name of the scorer.</param>
    /// <param name="userName">User making the assignment.</param>
    /// <returns>The updated match.</returns>
    Task<Match> AssignScorerAsync(Guid id, string scorerName, string userName);

    /// <summary>
    /// Updates the court location for a match.
    /// </summary>
    /// <param name="id">The match ID.</param>
    /// <param name="location">New court location.</param>
    /// <param name="userName">User making the update.</param>
    /// <returns>The updated match.</returns>
    Task<Match> UpdateLocationAsync(Guid id, string location, string userName);

    /// <summary>
    /// Updates the scheduled time for a match.
    /// </summary>
    /// <param name="id">The match ID.</param>
    /// <param name="scheduledTime">New scheduled time.</param>
    /// <param name="userName">User making the update.</param>
    /// <returns>The updated match.</returns>
    Task<Match> UpdateTimeAsync(Guid id, DateTime scheduledTime, string userName);

    /// <summary>
    /// Gets all updates/events for a match.
    /// </summary>
    /// <param name="matchId">The match ID.</param>
    /// <returns>List of match updates ordered by time.</returns>
    Task<List<MatchUpdate>> GetMatchUpdatesAsync(Guid matchId);

    /// <summary>
    /// Adds a new update/event to a match.
    /// </summary>
    /// <param name="update">The match update to add.</param>
    /// <returns>The created match update.</returns>
    Task<MatchUpdate> AddMatchUpdateAsync(MatchUpdate update);

    /// <summary>
    /// Checks if a team has already played in a specific round.
    /// </summary>
    /// <param name="teamId">The team ID to check.</param>
    /// <param name="roundId">The round ID to check.</param>
    /// <returns>True if team has played, false otherwise.</returns>
    Task<bool> HasTeamPlayedInRoundAsync(Guid teamId, Guid roundId);

    /// <summary>
    /// Checks if both teams are available at the specified time (no overlapping matches).
    /// </summary>
    /// <param name="homeTeamId">Home team ID.</param>
    /// <param name="awayTeamId">Away team ID.</param>
    /// <param name="scheduledTime">Proposed match time.</param>
    /// <returns>True if both teams are available, false otherwise.</returns>
    Task<bool> AreTeamsAvailableAsync(Guid homeTeamId, Guid awayTeamId, DateTime scheduledTime);

    /// <summary>
    /// Checks if a court is available at the specified time (no overlapping matches).
    /// </summary>
    /// <param name="courtLocation">Court location to check.</param>
    /// <param name="scheduledTime">Proposed match time.</param>
    /// <returns>True if court is available, false otherwise.</returns>
    Task<bool> IsCourtAvailableAsync(string courtLocation, DateTime scheduledTime);
    
    // MatchSet operations
    
    /// <summary>
    /// Gets all sets for a match.
    /// </summary>
    /// <param name="matchId">The match ID.</param>
    /// <returns>List of match sets ordered by set number.</returns>
    Task<List<MatchSet>> GetMatchSetsAsync(Guid matchId);

    /// <summary>
    /// Gets the currently active set for a match.
    /// </summary>
    /// <param name="matchId">The match ID.</param>
    /// <returns>The current set or null if no set is active.</returns>
    Task<MatchSet?> GetCurrentSetAsync(Guid matchId);

    /// <summary>
    /// Updates the score for a specific set.
    /// </summary>
    /// <param name="matchId">The match ID.</param>
    /// <param name="setNumber">The set number (1-5).</param>
    /// <param name="homeScore">New home team score for this set.</param>
    /// <param name="awayScore">New away team score for this set.</param>
    /// <param name="userId">User making the update.</param>
    /// <returns>The updated match set.</returns>
    Task<MatchSet> UpdateSetScoreAsync(Guid matchId, int setNumber, int homeScore, int awayScore, string userId);

    /// <summary>
    /// Marks a set as finished.
    /// </summary>
    /// <param name="matchId">The match ID.</param>
    /// <param name="setNumber">The set number to finish.</param>
    /// <param name="userId">User finishing the set.</param>
    /// <returns>The finished match set.</returns>
    Task<MatchSet> FinishSetAsync(Guid matchId, int setNumber, string userId);

    /// <summary>
    /// Starts the next set in a match.
    /// </summary>
    /// <param name="matchId">The match ID.</param>
    /// <param name="userId">User starting the set.</param>
    /// <param name="currentSetNumber">Current set number.</param>
    /// <returns>The newly started set.</returns>
    Task<MatchSet> StartNextSetAsync(Guid matchId, string userId, int currentSetNumber);

    /// <summary>
    /// Reverts to the previous set (undoes finishing a set).
    /// </summary>
    /// <param name="matchId">The match ID.</param>
    /// <param name="userId">User reverting the set.</param>
    /// <param name="currentSetNumber">Current set number.</param>
    /// <returns>The reverted set.</returns>
    Task<MatchSet> RevertToPreviousSetAsync(Guid matchId, string userId, int currentSetNumber);

    /// <summary>
    /// Ends a match and locks all sets (prevents further modifications).
    /// </summary>
    /// <param name="matchId">The match ID to end.</param>
    /// <param name="userId">User ending the match.</param>
    /// <returns>The ended and locked match.</returns>
    Task<Match> EndMatchAndLockSetsAsync(Guid matchId, string userId);

    /// <summary>
    /// Updates multiple match details in one operation.
    /// </summary>
    /// <param name="matchId">The match ID.</param>
    /// <param name="scheduledTime">Optional new scheduled time.</param>
    /// <param name="courtLocation">Optional new court location.</param>
    /// <param name="refereeName">Optional new referee name.</param>
    /// <param name="scorerName">Optional new scorer name.</param>
    /// <param name="userId">User making the updates.</param>
    /// <returns>The updated match.</returns>
    Task<Match> UpdateMatchDetailsAsync(Guid matchId, DateTime? scheduledTime, string? courtLocation, string? refereeName, string? scorerName, string userId);

    /// <summary>
    /// Gets a specific set for a match.
    /// </summary>
    /// <param name="matchId">The match ID.</param>
    /// <param name="currentSetNumber">The set number to retrieve.</param>
    /// <returns>The match set or null if not found.</returns>
    Task<MatchSet?> GetMatchSetAsync(Guid matchId, int currentSetNumber);

    /// <summary>
    /// Gets an existing set or creates it if it doesn't exist.
    /// </summary>
    /// <param name="matchId">The match ID.</param>
    /// <param name="setNumber">The set number.</param>
    /// <param name="userName">User requesting/creating the set.</param>
    /// <returns>The existing or newly created match set.</returns>
    Task<MatchSet> GetOrCreateMatchSetAsync(Guid matchId, int setNumber, string userName);

    /// <summary>
    /// Checks if a match has been called to court (teams should report).
    /// </summary>
    /// <param name="matchId">The match ID to check.</param>
    /// <returns>True if match is called to court, false otherwise.</returns>
    Task<bool> IsCalledToCourt(Guid matchId);

    /// <summary>
    /// Gets the most recent match updates across all matches.
    /// </summary>
    /// <param name="count">Number of recent updates to retrieve (default: 25).</param>
    /// <returns>List of recent match updates ordered by time.</returns>
    Task<List<MatchUpdate>> GetRecentMatchUpdatesAsync(int count = 25);

    /// <summary>
    /// Deletes all matches for a specific tournament (used for cleanup/reset).
    /// </summary>
    /// <param name="tournamentId">The tournament ID.</param>
    /// <returns>Number of matches deleted.</returns>
    Task<int> DeleteAllMatchesByTournamentAsync(Guid tournamentId);
}
