using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public interface IAnnouncementService
{
    Task<Announcement?> GetAnnouncementByIdAsync(Guid id);
    Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync(bool includeHidden = false);
    Task<Announcement> CreateAnnouncementAsync(Announcement announcement);
    Task<Announcement> UpdateAnnouncementAsync(Announcement announcement);
    Task<Announcement> UpdateAnnouncementVisibilityAsync(Guid id, bool isVisible);
    Task<bool> DeleteAnnouncementAsync(Guid id);
    Task<IEnumerable<Announcement>> GetRecentAnnouncementsAsync(int count);
}
