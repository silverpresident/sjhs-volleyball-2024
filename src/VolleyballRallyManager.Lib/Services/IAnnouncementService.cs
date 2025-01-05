using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;
using Markdig;

namespace VolleyballRallyManager.Lib.Services;

public interface IAnnouncementService
{
    Task<Announcement?> GetAnnouncementByIdAsync(Guid id);
    Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync(bool includeHidden = false);
    Task<Announcement> CreateAnnouncementAsync(Announcement announcement);
    Task<Announcement> UpdateAnnouncementAsync(Announcement announcement);
    Task<Announcement> UpdateAnnouncementVisibilityAsync(Guid id, bool isVisible);
    Task<bool> DeleteAnnouncementAsync(Guid id);
}
