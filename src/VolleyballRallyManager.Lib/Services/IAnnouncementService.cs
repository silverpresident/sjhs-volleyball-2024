using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public interface IAnnouncementService
{
    Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync(bool includeHidden = false);
    Task<Announcement?> GetAnnouncementByIdAsync(Guid id);
    Task<IEnumerable<Announcement>> GetQueuedAnnouncementsAsync();
    Task<Announcement> CreateAnnouncementAsync(Announcement announcement);
    Task<Announcement> UpdateAnnouncementAsync(Announcement announcement);
    Task<bool> DeleteAnnouncementAsync(Guid id);
    Task<Announcement> HideAnnouncementAsync(Guid id);
    Task<Announcement> UnhideAnnouncementAsync(Guid id);
    Task<Announcement> CallAnnouncementAsync(Guid id);
    Task<Announcement> DeferAnnouncementAsync(Guid id);
    Task<Announcement> ReannounceAsync(Guid id);
    Task<IEnumerable<AnnouncementHistoryLog>> GetHistoryForAnnouncementAsync(Guid id);
}
