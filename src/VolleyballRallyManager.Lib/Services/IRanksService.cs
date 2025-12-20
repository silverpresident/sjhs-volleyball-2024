using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

/// <summary>
/// Service for calculating and updating team rankings within tournament rounds
/// </summary>
public interface IRanksService
{
    /// <summary>
    /// Updates team ranks for a specific tournament round based on match results
    /// Applies strict tie-breaker rules:
    /// 1. Highest Total Points (Win=3, Draw=1, Loss=0)
    /// 2. Highest Score Difference (ScoreDifference)
    /// 3. Highest Score For (ScoreFor)
    /// 4. Best Seed Number (lower seed number is better)
    /// </summary>
    /// <param name="tournamentRoundId">The ID of the tournament round to update rankings for</param>
    /// <returns>Updated list of tournament round teams with calculated rankings</returns>
    Task<List<TournamentRoundTeam>> UpdateTeamRanksAsync(Guid tournamentRoundId);

    /// <summary>
    /// Calculates the final rank for a specific team within a round without persisting changes
    /// </summary>
    /// <param name="tournamentRoundId">The ID of the tournament round</param>
    /// <param name="teamId">The ID of the team to calculate rank for</param>
    /// <returns>The calculated final rank</returns>
    Task<int> CalculateTeamRankAsync(Guid tournamentRoundId, Guid teamId);

    /// <summary>
    /// Gets the current standings for a tournament round
    /// </summary>
    /// <param name="tournamentRoundId">The ID of the tournament round</param>
    /// <returns>List of teams ordered by their current standings</returns>
    Task<List<TournamentRoundTeam>> GetStandingsAsync(Guid tournamentRoundId);

    /// <summary>
    /// Updates team ranks for a specific tournament division based on ranking points for each round
    /// </summary>
    /// <param name="tournamentId">The ID of the tournament to update rankings for</param>
    /// <param name="divisionId">The ID of the tournament division to update rankings for</param>
    /// <returns>Updated list of tournament division teams with calculated rankings</returns>

    Task<List<TournamentTeamDivision>> UpdateDivisionRanksAsync(Guid tournamentId, Guid divisionId);


    /// <summary>
    /// Updates team statistics after a match is completed.
    /// Recalculates wins, losses, points, and other statistics.
    /// </summary>
    /// <param name="match">The completed match to process.</param>
    Task UpdateTeamStatisticsAsync(Match match);
}
