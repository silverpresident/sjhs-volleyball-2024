using VolleyballRallyManager.Lib.Common;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services
{
    /// <summary>
    /// Service for managing the active tournament and its related operations.
    /// Provides access to tournament teams, divisions, matches, and statistics.
    /// </summary>
    public interface IActiveTournamentService
    {
        /// <summary>
        /// Retrieves the currently active tournament.
        /// </summary>
        /// <returns>The active tournament or null if none exists.</returns>
        Task<Tournament?> GetActiveTournamentAsync();

        /// <summary>
        /// Gets all divisions associated with the active tournament.
        /// </summary>
        /// <returns>Collection of tournament divisions.</returns>
        Task<IEnumerable<TournamentDivision>> GetTournamentDivisionsAsync();

        /// <summary>
        /// Gets all teams in the active tournament, optionally filtered by division.
        /// </summary>
        /// <param name="divisionId">Division ID to filter by, or Guid.Empty for all divisions.</param>
        /// <returns>Collection of tournament team divisions.</returns>
        Task<IEnumerable<TournamentTeamDivision>> GetTournamentTeamsAsync(Guid divisionId);

        /// <summary>
        /// Gets the count of teams in the active tournament for a specific division.
        /// </summary>
        /// <param name="divisionId">The division ID to count teams for.</param>
        /// <returns>Number of teams in the division.</returns>
        Task<int> GetTournamentTeamsCountAsync(Guid divisionId);

        /// <summary>
        /// Retrieves a specific team from the active tournament.
        /// </summary>
        /// <param name="teamId">The team ID to retrieve.</param>
        /// <returns>The tournament team division or null if not found.</returns>
        Task<TournamentTeamDivision?> GetTeamAsync(Guid teamId);

        /// <summary>
        /// Removes a team from the active tournament.
        /// </summary>
        /// <param name="teamId">The team ID to remove.</param>
        Task RemoveTeamAsync(Guid teamId);

        /// <summary>
        /// Adds a team to the active tournament in a specific division.
        /// </summary>
        /// <param name="teamId">The team ID to add.</param>
        /// <param name="divisionId">The division to add the team to.</param>
        /// <param name="groupName">Optional group name for the team.</param>
        /// <param name="seedNumber">Optional seed number (0 = unseeded).</param>
        /// <param name="rating">Optional rating for the team.</param>
        /// <returns>The created tournament team division.</returns>
        Task<TournamentTeamDivision> AddTeamAsync(Guid teamId, Guid divisionId, string groupName, int seedNumber = 0, int rating = 0);

        /// <summary>
        /// Updates an existing team's division assignment and properties.
        /// </summary>
        /// <param name="teamId">The team ID to update.</param>
        /// <param name="divisionId">The new division ID.</param>
        /// <param name="groupName">Updated group name.</param>
        /// <param name="seedNumber">Updated seed number.</param>
        /// <param name="rating">Updated rating.</param>
        /// <returns>The updated tournament team division.</returns>
        Task<TournamentTeamDivision> SetTeamAsync(Guid teamId, Guid divisionId, string groupName, int seedNumber = 0, int rating = 0);

        /// <summary>
        /// Gets the count of matches in the active tournament, optionally filtered by state and division.
        /// </summary>
        /// <param name="matchState">Match state filter (None = all states).</param>
        /// <param name="divisionId">Optional division filter.</param>
        /// <returns>Number of matches matching the criteria.</returns>
        Task<int> MatchCountAsync(MatchState matchState = MatchState.None, Guid? divisionId = null);

        /// <summary>
        /// Gets the count of teams in the active tournament, optionally filtered by division.
        /// </summary>
        /// <param name="divisionId">Optional division filter.</param>
        /// <returns>Number of teams matching the criteria.</returns>
        Task<int> TeamCountAsync(Guid? divisionId = null);


        /// <summary>
        /// Gets all divisions available for the active tournament.
        /// </summary>
        /// <returns>Collection of available divisions.</returns>
        Task<IEnumerable<Division>> GetAvailableDivisionsAsync();

        /// <summary>
        /// Gets all teams available to be added to the active tournament.
        /// </summary>
        /// <returns>Collection of available teams.</returns>
        Task<IEnumerable<Team>> GetAvailableTeamsAsync();

        /// <summary>
        /// Gets matches from the active tournament with optional filters.
        /// </summary>
        /// <param name="divisionId">Optional division filter.</param>
        /// <param name="roundId">Optional round filter.</param>
        /// <param name="groupName">Optional group name filter.</param>
        /// <param name="teamId">Optional team filter.</param>
        /// <returns>Collection of matches matching the criteria.</returns>
        Task<IEnumerable<Match>> GetMatchesAsync(Guid? divisionId = null, Guid? roundId = null, string? groupName = null, Guid? teamId = null);

        /// <summary>
        /// Updates which divisions are associated with the active tournament.
        /// </summary>
        /// <param name="selectedDivisionIds">List of division IDs to associate with the tournament.</param>
        Task UpdateTournamentDivisionsAsync(List<Guid> selectedDivisionIds);

        /// <summary>
        /// Gets the most recent matches from the active tournament.
        /// </summary>
        /// <param name="howMany">Number of recent matches to retrieve (default: 10).</param>
        /// <returns>Collection of recent matches, ordered by time.</returns>
        Task<IEnumerable<Match>> RecentMatchesAsync(int howMany = 10);

        /// <summary>
        /// Retrieves a specific division by ID.
        /// </summary>
        /// <param name="divisionId">The division ID to retrieve.</param>
        /// <returns>The division or null if not found.</returns>
        Task<Division?> GetDivisionAsync(Guid divisionId);

        /// <summary>
        /// Gets the scheduled start time of the next upcoming match.
        /// </summary>
        /// <returns>The start time of the next match.</returns>
        Task<DateTime> GetNextMatchStartTimeAsync();
    }
}
