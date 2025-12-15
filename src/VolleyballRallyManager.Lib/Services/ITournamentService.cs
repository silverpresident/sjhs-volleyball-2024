using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services
{
    /// <summary>
    /// Service for managing tournaments, their divisions, teams, and comprehensive tournament data aggregation.
    /// Provides high-level tournament operations and complex view model generation for display purposes.
    /// </summary>
    public interface ITournamentService
    {
        /// <summary>
        /// Creates a new tournament.
        /// </summary>
        /// <param name="tournament">The tournament to create.</param>
        Task CreateTournamentAsync(Tournament tournament);

        /// <summary>
        /// Sets a tournament as the active tournament.
        /// Only one tournament can be active at a time.
        /// </summary>
        /// <param name="tournamentId">The tournament ID to set as active.</param>
        Task SetActiveTournamentAsync(Guid tournamentId);

        /// <summary>
        /// Gets all divisions associated with a specific tournament.
        /// </summary>
        /// <param name="tournamentId">The tournament ID.</param>
        /// <returns>Collection of divisions in the tournament.</returns>
        Task<IEnumerable<Division>> GetTournamentDivisionsAsync(Guid tournamentId);

        /// <summary>
        /// Gets all teams participating in a specific tournament.
        /// </summary>
        /// <param name="tournamentId">The tournament ID.</param>
        /// <returns>Collection of teams in the tournament.</returns>
        Task<IEnumerable<Team>> GetTournamentTeamsAsync(Guid tournamentId);

        /// <summary>
        /// Gets all matches in a specific tournament.
        /// </summary>
        /// <param name="tournamentId">The tournament ID.</param>
        /// <returns>Collection of matches in the tournament.</returns>
        Task<IEnumerable<Match>> GetTournamentMatchesAsync(Guid tournamentId);

        /// <summary>
        /// Adds a division to a tournament.
        /// </summary>
        /// <param name="tournamentId">The tournament ID.</param>
        /// <param name="divisionId">The division ID to add.</param>
        Task AddDivisionToTournamentAsync(Guid tournamentId, Guid divisionId);

        /// <summary>
        /// Removes a division from a tournament.
        /// </summary>
        /// <param name="tournamentId">The tournament ID.</param>
        /// <param name="divisionId">The division ID to remove.</param>
        Task RemoveDivisionFromTournamentAsync(Guid tournamentId, Guid divisionId);

        /// <summary>
        /// Adds a team to a tournament in a specific division.
        /// </summary>
        /// <param name="tournamentId">The tournament ID.</param>
        /// <param name="teamId">The team ID to add.</param>
        /// <param name="divisionId">The division to add the team to.</param>
        /// <param name="group">Optional group name for team.</param>
        /// <param name="seedNumber">Optional seed number for team.</param>
        Task AddTeamToTournamentAsync(Guid tournamentId, Guid teamId, Guid divisionId, string group, int seedNumber);

        /// <summary>
        /// Removes a team from a tournament.
        /// </summary>
        /// <param name="tournamentId">The tournament ID.</param>
        /// <param name="teamId">The team ID to remove.</param>
        Task RemoveTeamFromTournamentAsync(Guid tournamentId, Guid teamId);

        /// <summary>
        /// Retrieves the currently active tournament.
        /// </summary>
        /// <returns>The active tournament or null if none exists.</returns>
        Task<Tournament?> GetActiveTournamentAsync();

        /// <summary>
        /// Retrieves a specific tournament by ID.
        /// </summary>
        /// <param name="tournamentId">The tournament ID to retrieve.</param>
        /// <returns>The tournament or null if not found.</returns>
        Task<Tournament?> GetTournamentByIdAsync(Guid tournamentId);

        /// <summary>
        /// Gets all tournaments in the system.
        /// </summary>
        /// <returns>Collection of all tournaments.</returns>
        Task<IEnumerable<Tournament>> GetAllTournamentsAsync();

        /// <summary>
        /// Updates an existing tournament.
        /// </summary>
        /// <param name="tournament">The tournament with updated values.</param>
        Task UpdateTournamentAsync(Tournament tournament);

        /// <summary>
        /// Gets all teams in a specific division.
        /// </summary>
        /// <param name="division">The division to get teams for.</param>
        /// <returns>Collection of teams in the division.</returns>
        Task<IEnumerable<Team>> GetTeamsByDivisionAsync(Division division);

        /// <summary>
        /// Gets the leaderboard (ranked teams) for a specific division.
        /// </summary>
        /// <param name="divisionId">The division ID.</param>
        /// <returns>Collection of teams ordered by rank.</returns>
        Task<IEnumerable<Team>> GetLeaderboardAsync(Guid divisionId);

        /// <summary>
        /// Recalculates cumulative statistics for a team across all matches.
        /// Updates wins, losses, points, score differences, etc.
        /// </summary>
        /// <param name="teamId">The team ID to recalculate statistics for.</param>
        Task RecalculateTeamStatisticsAsync(Guid teamId);

        /// <summary>
        /// Loads detailed tournament information (legacy method).
        /// </summary>
        /// <param name="tournament">The tournament to load details for.</param>
        Task GetDetailsAsync(Tournament tournament);

        /// <summary>
        /// Generates a comprehensive tournament details view model with divisions, teams, and statistics.
        /// Used for tournament overview pages.
        /// </summary>
        /// <param name="tournamentId">The tournament ID.</param>
        /// <returns>View model containing complete tournament details.</returns>
        Task<TournamentDetailsViewModel> GetTournamentDetailsAsync(Guid tournamentId);

        /// <summary>
        /// Generates a summary view model for a specific tournament round.
        /// Includes round info, team count, match count, and completion status.
        /// </summary>
        /// <param name="tournamentRoundId">The tournament round ID.</param>
        /// <returns>View model with round summary or null if not found.</returns>
        Task<TournamentRoundSummaryViewModel?> GetTournamentRoundSummaryAsync(Guid tournamentRoundId);

        /// <summary>
        /// Generates detailed view model for a tournament round with teams and matches.
        /// Includes rankings, statistics, and complete match information.
        /// </summary>
        /// <param name="tournamentRoundId">The tournament round ID.</param>
        /// <returns>View model with complete round details or null if not found.</returns>
        Task<TournamentRoundDetailsViewModel?> GetTournamentRoundDetailsAsync(Guid tournamentRoundId);

        /// <summary>
        /// Generates detailed view model for a tournament division.
        /// Includes teams, rounds, matches, and division-specific statistics.
        /// </summary>
        /// <param name="tournamentId">The tournament ID.</param>
        /// <param name="divisionId">The division ID.</param>
        /// <returns>View model with complete division details.</returns>
        Task<TournamentDivisionDetailsViewModel> GetTournamentDivisionDetailsAsync(Guid tournamentId, Guid divisionId);
    }
}
