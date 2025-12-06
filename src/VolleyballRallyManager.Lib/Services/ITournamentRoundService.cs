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
    /// <param name="roundId">Round ID</param>
    /// <param name="advancingTeamSelectionStrategy">Method for selecting teams</param>
    /// <param name="matchGenerationStrategy">Strategy for generating matches</param>
    /// <param name="advancingTeamsCount">Number of Teams Advancing for next round</param>
    /// <param name="userName">User creating the round</param>
    /// <returns>Created tournament round</returns>
    Task<TournamentRound> CreateFirstRoundAsync(
        Guid tournamentId, 
        Guid divisionId, 
        Guid roundId,
        int advancingTeamsCount,
        TeamSelectionStrategy advancingTeamSelectionStrategy,
        MatchGenerationStrategy matchGenerationStrategy,
        string userName);

    /// <summary>
    /// Creates a subsequent round
    /// </summary>
    /// <param name="tournamentId">Tournament ID</param>
    /// <param name="divisionId">Division ID</param>
    /// <param name="roundId">Round ID</param>
    /// <param name="previousTournamentRoundId">Previous round ID</param>
    /// <param name="advancingTeamSelectionStrategy">Method for selecting teams</param>
    /// <param name="matchGenerationStrategy">Strategy for generating matches</param>
    /// <param name="TeamsAdvancing">Number of Teams Advancing for next round</param>
    /// <param name="userName">User creating the round</param>
    /// <returns>Created tournament round</returns>
    Task<TournamentRound> CreateNextRoundAsync(
        Guid tournamentId,
        Guid divisionId,
        Guid roundId,
        Guid previousTournamentRoundId,
        TeamSelectionStrategy advancingTeamSelectionStrategy,
        MatchGenerationStrategy matchGenerationStrategy,
        int TeamsAdvancing,
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
    /// Checks if all matches in a round are complete
    /// </summary>
    Task<bool> AreAllMatchesCompleteAsync(Guid tournamentRoundId);

    /// <summary>
    /// Checks if a round has teams assigned
    /// </summary>
    Task<bool> HasTeamsAssignedAsync(Guid tournamentRoundId);

    /// <summary>
    /// Checks if a round has matches generated
    /// </summary>
    Task<bool> HasMatchesGeneratedAsync(Guid tournamentRoundId);

    /// <summary>
    /// Gets teams for a tournament round
    /// </summary>
    Task<List<TournamentRoundTeam>> GetRoundTeamsAsync(Guid tournamentRoundId);

    /// <summary>
    /// Gets matches for a tournament round
    /// </summary>
    Task<List<Match>> GetRoundMatchesAsync(Guid tournamentRoundId);

    /// <summary>
    /// Deletes all rounds for a specific tournament
    /// </summary>
    /// <param name="tournamentId">Tournament ID</param>
    /// <returns>Number of rounds deleted</returns>
    Task<int> DeleteAllRoundsByTournamentAsync(Guid tournamentId);
}
