using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

/// <summary>
/// Service for managing tournament rounds lifecycle
/// </summary>
public interface ITournamentRoundService
{
    /// <summary>
    /// Gets all tournament rounds for a specific tournament and division
    /// </summary>
    Task<List<TournamentRound>> GetTournamentRoundsAsync(Guid tournamentId, Guid? divisionId);

    /// <summary>
    /// Gets a specific tournament round by ID with all related data
    /// </summary>
    Task<TournamentRound?> GetTournamentRoundByIdAsync(Guid tournamentRoundId);

    /// <summary>
    /// Creates the first round for a tournament division
    /// </summary>
    /// <param name="tournamentId">Tournament ID</param>
    /// <param name="divisionId">Division ID</param>
    /// <param name="roundId">CurrentRound ID</param>
    /// <param name="advancingTeamSelectionStrategy">Method for selecting teams</param>
    /// <param name="matchGenerationStrategy">Strategy for generating matches</param>
    /// <param name="advancingTeamsCount">Number of Teams Advancing for next round</param>
    /// <param name="groupingStrategy">Strategy for generating groups</param>
    /// <param name="groupingSize">Teams per group or groups per round</param>
    /// <param name="userName">User creating the round</param>
    /// <returns>Created tournament round</returns>
    Task<TournamentRound> CreateFirstRoundAsync(
        Guid tournamentId, 
        Guid divisionId, 
        Guid roundId,
        int advancingTeamsCount,
        TeamSelectionStrategy advancingTeamSelectionStrategy,
        MatchGenerationStrategy matchGenerationStrategy,
        GroupGenerationStrategy groupingStrategy,
        int groupingSize,
        string userName);

    /// <summary>
    /// Creates a subsequent round
    /// </summary>
    /// <param name="tournamentId">Tournament ID</param>
    /// <param name="divisionId">Division ID</param>
    /// <param name="roundId">CurrentRound ID</param>
    /// <param name="previousTournamentRoundId">Previous round ID</param>
    /// <param name="qualifyingTeamsCount">Number of Teams Advancing for next round</param>
    /// <param name="qualifyingTeamSelectionStrategy">Method for selecting teams</param>
    /// <param name="advancingTeamsCount">Number of Teams Advancing for next round</param>
    /// <param name="advancingTeamSelectionStrategy">Method for selecting teams</param>
    /// <param name="matchGenerationStrategy">Strategy for generating matches</param>
    /// <param name="groupingStrategy">Strategy for generating groups</param>
    /// <param name="groupingSize">Teams per group or groups per round</param>
    /// <param name="isPlayoff">Is playoff round</param>
    /// <param name="userName">User creating the round</param>
    /// <returns>Created tournament round</returns>
    Task<TournamentRound> CreateNextRoundAsync(
        Guid tournamentId,
        Guid divisionId,
        Guid roundId,
        Guid previousTournamentRoundId,
        int qualifyingTeamsCount,
        TeamSelectionStrategy qualifyingTeamSelectionStrategy,
        int advancingTeamsCount,
        TeamSelectionStrategy advancingTeamSelectionStrategy,
        MatchGenerationStrategy matchGenerationStrategy,
        GroupGenerationStrategy groupingStrategy,
        int groupingSize,
        bool isPlayoff,
        string userName);

    /// <summary>
    /// Assigns and seeds teams for a divisions first round
    /// </summary>
    /// <param name="tournamentRoundId">Tournament round ID</param>
    /// <param name="userName">User performing the action</param>
    /// <returns>List of selected and seeded teams</returns>
    Task<List<TournamentRoundTeam>> AssignFirstRoundTeamsAsync(Guid tournamentRoundId, string userName);

    /// <summary>
    /// Selects and seeds teams for a tournament round
    /// </summary>
    /// <param name="tournamentRoundId">Tournament round ID</param>
    /// <param name="userName">User performing the action</param>
    /// <returns>List of selected and seeded teams</returns>
    Task<List<TournamentRoundTeam>> SelectTeamsForRoundAsync(Guid tournamentRoundId, string userName);

    /// <summary>
    /// Generates matches for a tournament round based on its strategy
    /// </summary>
    /// <param name="tournamentRoundId">Tournament round ID</param>
    /// <param name="startTime">Starting time for matches</param>
    /// <param name="startingCourtNumber">First court number to use</param>
    /// <param name="numberOfCourts">Number of courts available</param>
    /// <param name="matchTimeInterval">Time interval between matches on the same court (in minutes)</param>
    /// <param name="userName">User performing the action</param>
    /// <returns>List of generated matches</returns>
    Task<List<Match>> GenerateMatchesForRoundAsync(
        Guid tournamentRoundId, 
        DateTime startTime, 
        int startingCourtNumber,
        int numberOfCourts,
        int matchTimeInterval,
        string userName);

    /// <summary>
    /// Finalizes a tournament round (calculates rankings and locks it)
    /// </summary>
    /// <param name="tournamentRoundId">Tournament round ID</param>
    /// <param name="userName">User performing the action</param>
    /// <returns>Finalized tournament round</returns>
    Task<TournamentRound> FinalizeRoundAsync(Guid tournamentRoundId, string userName);

    /// <summary>
    /// Checks if all matches in a round are complete.
    /// </summary>
    /// <param name="tournamentRoundId">The tournament round ID to check.</param>
    /// <returns>True if all matches are complete, false otherwise.</returns>
    Task<bool> AreAllMatchesCompleteAsync(Guid tournamentRoundId);

    /// <summary>
    /// Checks if a round has teams assigned to it.
    /// </summary>
    /// <param name="tournamentRoundId">The tournament round ID to check.</param>
    /// <returns>True if teams are assigned, false otherwise.</returns>
    Task<bool> HasTeamsAssignedAsync(Guid tournamentRoundId);

    /// <summary>
    /// Checks if a round has matches generated.
    /// </summary>
    /// <param name="tournamentRoundId">The tournament round ID to check.</param>
    /// <returns>True if matches exist, false otherwise.</returns>
    Task<bool> HasMatchesGeneratedAsync(Guid tournamentRoundId);

    /// <summary>
    /// Gets all teams participating in a tournament round with their statistics.
    /// </summary>
    /// <param name="tournamentRoundId">The tournament round ID.</param>
    /// <returns>List of tournament round teams with rankings and statistics.</returns>
    Task<List<TournamentRoundTeam>> GetRoundTeamsAsync(Guid tournamentRoundId);

    /// <summary>
    /// Gets all matches in a tournament round.
    /// </summary>
    /// <param name="tournamentRoundId">The tournament round ID.</param>
    /// <returns>List of matches in the round.</returns>
    Task<List<Match>> GetRoundMatchesAsync(Guid tournamentRoundId);

    /// <summary>
    /// Deletes all rounds for a specific tournament (developer/cleanup operation).
    /// </summary>
    /// <param name="tournamentId">The tournament ID.</param>
    /// <returns>Number of rounds deleted.</returns>
    Task<int> DeleteAllRoundsByTournamentAsync(Guid tournamentId);

    /// <summary>
    /// Unfinalizes a previously finalized round (unlocks it for modifications).
    /// Use with caution as this can affect dependent rounds.
    /// </summary>
    /// <param name="id">The tournament round ID to unfinalize.</param>
    /// <param name="userName">User performing the action.</param>
    /// <returns>The unfinalized tournament round.</returns>
    Task<TournamentRound> UnfinalizeRoundAsync(Guid id, string userName);

    /// <summary>
    /// Gets playoff candidate teams from a previous round (best losers)
    /// </summary>
    /// <param name="previousRoundId">The ID of the completed round</param>
    /// <param name="numberOfTeamsToSelect">Number of teams to select</param>
    /// <returns>List of best performing non-advancing teams</returns>
    Task<IEnumerable<TournamentRoundTeamSummaryViewModel>> GetPlayoffCandidateTeamsAsync(Guid previousRoundId, int numberOfTeamsToSelect);

    /// <summary>
    /// Add teams to a tournament round.
    /// </summary>
    /// <param name="tournamentRoundId">The tournament round ID.</param>
    /// <returns>none</returns>
    Task AddTeamsToRoundAsync(Guid tournamentRoundId, List<Guid> selectedTeamIds, string userName);
}
