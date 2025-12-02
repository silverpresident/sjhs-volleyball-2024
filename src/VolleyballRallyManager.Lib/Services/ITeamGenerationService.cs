using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public interface ITeamGenerationService
{
    Task<IEnumerable<Team>> GenerateRandomTeamsAsync(int count);
}
