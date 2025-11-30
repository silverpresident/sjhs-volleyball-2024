using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public interface IBulletinService
{
    Task<Bulletin?> GetBulletinByIdAsync(Guid id);
    Task<IEnumerable<Bulletin>> GetAllBulletinsAsync(bool includeHidden = false);
    Task<Bulletin> CreateBulletinAsync(Bulletin bulletin);
    Task<Bulletin> UpdateBulletinAsync(Bulletin bulletin);
    Task<Bulletin> UpdateVisibilityAsync(Guid id, bool isVisible);
    Task<bool> DeleteBulletinAsync(Guid id);
    Task<IEnumerable<Bulletin>> GetRecentAsync(int count);
}
