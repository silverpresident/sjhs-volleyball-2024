using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public class BulletinService : IBulletinService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BulletinService> _logger;
    private readonly MarkdownPipeline _markdownPipeline;

    public BulletinService(
        ApplicationDbContext context,
        ILogger<BulletinService> logger)
    {
        _context = context;
        _logger = logger;
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public async Task<Bulletin?> GetBulletinByIdAsync(Guid id)
    {
        return await _context.Bulletins.FindAsync(id);
    }

    public async Task<IEnumerable<Bulletin>> GetAllBulletinsAsync(bool includeHidden = false)
    {
        var query = _context.Bulletins.AsQueryable();

        if (!includeHidden)
        {
            query = query.Where(a => a.IsVisible);
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Bulletin> CreateBulletinAsync(Bulletin bulletin)
    {
        if (bulletin.UseMarkdown)
        {
            bulletin.RenderedContent = Markdown.ToHtml(bulletin.Content, _markdownPipeline);
        }
        else
        {
            bulletin.RenderedContent = string.Empty;
        }

        _context.Bulletins.Add(bulletin);
        await _context.SaveChangesAsync();

        return bulletin;
    }

    public async Task<Bulletin> UpdateBulletinAsync(Bulletin bulletin)
    {
        var existingBulletin = await GetBulletinByIdAsync(bulletin.Id) ??
            throw new InvalidOperationException($"Bulletin with ID {bulletin.Id} not found.");

        if (bulletin.UseMarkdown)
        {
            bulletin.RenderedContent = Markdown.ToHtml(bulletin.Content, _markdownPipeline);
        }
        else
        {
            bulletin.RenderedContent = string.Empty;
        }

        _context.Entry(existingBulletin).CurrentValues.SetValues(bulletin);
        await _context.SaveChangesAsync();

        return bulletin;
    }

    public async Task<Bulletin> UpdateVisibilityAsync(Guid id, bool isVisible)
    {
        var bulletin = await GetBulletinByIdAsync(id) ??
            throw new InvalidOperationException($"Bulletin with ID {id} not found.");

        bulletin.IsVisible = isVisible;
        await _context.SaveChangesAsync();

        return bulletin;
    }

    public async Task<bool> DeleteBulletinAsync(Guid id)
    {
        var bulletin = await GetBulletinByIdAsync(id);
        if (bulletin == null)
        {
            return false;
        }

        _context.Bulletins.Remove(bulletin);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Bulletin>> GetRecentAsync(int count)
    {
        return await _context.Bulletins
                            .OrderByDescending(a => a.CreatedAt)
                            .Where(a => a.IsVisible == true)
                            .Take(count)
                            .ToListAsync();
    }
}
