using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;
using Markdig;

namespace VolleyballRallyManager.Lib.Services;

public interface IAnnouncementService
{
    Task<Announcement?> GetAnnouncementAsync(Guid id);
    Task<IEnumerable<Announcement>> GetAnnouncementsAsync(bool includeHidden = false);
    Task<Announcement> CreateAnnouncementAsync(Announcement announcement);
    Task<Announcement> UpdateAnnouncementAsync(Announcement announcement);
    Task<Announcement> UpdateAnnouncementVisibilityAsync(Guid id, bool isVisible);
    Task<bool> DeleteAnnouncementAsync(Guid id);
}

public class AnnouncementService : IAnnouncementService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AnnouncementService> _logger;
    private readonly MarkdownPipeline _markdownPipeline;

    public AnnouncementService(
        ApplicationDbContext context,
        ILogger<AnnouncementService> logger)
    {
        _context = context;
        _logger = logger;
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public async Task<Announcement?> GetAnnouncementAsync(Guid id)
    {
        return await _context.Announcements.FindAsync(id);
    }

    public async Task<IEnumerable<Announcement>> GetAnnouncementsAsync(bool includeHidden = false)
    {
        var query = _context.Announcements.AsQueryable();

        if (!includeHidden)
        {
            query = query.Where(a => a.IsVisible);
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
    {
        if (announcement.UseMarkdown)
        {
            announcement.RenderedContent = Markdown.ToHtml(announcement.Content, _markdownPipeline);
        }
        else
        {
            announcement.RenderedContent = string.Empty;
        }

        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();

        return announcement;
    }

    public async Task<Announcement> UpdateAnnouncementAsync(Announcement announcement)
    {
        var existingAnnouncement = await GetAnnouncementAsync(announcement.Id) ??
            throw new InvalidOperationException($"Announcement with ID {announcement.Id} not found.");

        if (announcement.UseMarkdown)
        {
            announcement.RenderedContent = Markdown.ToHtml(announcement.Content, _markdownPipeline);
        }
        else
        {
            announcement.RenderedContent = string.Empty;
        }

        _context.Entry(existingAnnouncement).CurrentValues.SetValues(announcement);
        await _context.SaveChangesAsync();

        return announcement;
    }

    public async Task<Announcement> UpdateAnnouncementVisibilityAsync(Guid id, bool isVisible)
    {
        var announcement = await GetAnnouncementAsync(id) ??
            throw new InvalidOperationException($"Announcement with ID {id} not found.");

        announcement.IsVisible = isVisible;
        await _context.SaveChangesAsync();

        return announcement;
    }

    public async Task<bool> DeleteAnnouncementAsync(Guid id)
    {
        var announcement = await GetAnnouncementAsync(id);
        if (announcement == null)
        {
            return false;
        }

        _context.Announcements.Remove(announcement);
        await _context.SaveChangesAsync();

        return true;
    }
}
