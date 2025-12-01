using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public interface IRoundService
{
    Task<IEnumerable<Round>> GetAllRoundsAsync();
    Task<Round?> GetRoundByIdAsync(Guid id);
    Task<Round?> GetRoundWithMatchesAsync(Guid id);
    Task<IEnumerable<Round>> GetRoundsWithMatchesAsync();
    Task CreateRoundAsync(Round round);
    Task UpdateRoundAsync(Round round);
    Task DeleteRoundAsync(Guid id);
    Task<int> GetMatchCountForRoundAsync(Guid roundId);
    Task<int> GetCompletedMatchCountForRoundAsync(Guid roundId);
}
