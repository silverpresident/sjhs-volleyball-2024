using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly ApplicationDbContext _context;
    private readonly ISignalRNotificationService _signalRService;
    private readonly ILogger<AnnouncementService> _logger;

    public AnnouncementService(
        ApplicationDbContext context,
        ISignalRNotificationService signalRService,
        ILogger<AnnouncementService> logger)
    {
        _context = context;
        _signalRService = signalRService;
        _logger = logger;
    }

    public async Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync(bool includeHidden = false)
    {
        try
        {
            var query = _context.Announcements.AsQueryable();

            if (!includeHidden)
            {
                query = query.Where(a => !a.IsHidden);
            }

            return await query
                .OrderBy(a => a.SequencingNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all announcements");
            throw;
        }
    }

    public async Task<Announcement?> GetAnnouncementByIdAsync(Guid id)
    {
        try
        {
            return await _context.Announcements
                .Include(a => a.HistoryLogs)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving announcement {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Announcement>> GetQueuedAnnouncementsAsync()
    {
        try
        {
            return await _context.Announcements
                .Where(a => !a.IsHidden)
                .OrderBy(a => a.SequencingNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving queued announcements");
            throw;
        }
    }

    public async Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
    {
        try
        {
            announcement.UpdatedAt = DateTime.UtcNow;

            // Assign sequence number based on priority
            if (announcement.Priority == AnnouncementPriority.Urgent)
            {
                // Insert after last non-hidden Urgent item
                var lastUrgent = await _context.Announcements
                    .Where(a => !a.IsHidden && a.Priority == AnnouncementPriority.Urgent)
                    .OrderByDescending(a => a.SequencingNumber)
                    .FirstOrDefaultAsync();

                if (lastUrgent != null)
                {
                    // Insert after the last urgent item
                    announcement.SequencingNumber = lastUrgent.SequencingNumber + 1;

                    // Re-sequence all items after this position
                    var itemsToResequence = await _context.Announcements
                        .Where(a => !a.IsHidden && a.SequencingNumber >= announcement.SequencingNumber)
                        .ToListAsync();

                    foreach (var item in itemsToResequence)
                    {
                        item.SequencingNumber++;
                        item.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    // First urgent item or no urgent items exist
                    announcement.SequencingNumber = 1;

                    // Re-sequence all existing items
                    var existingItems = await _context.Announcements
                        .Where(a => !a.IsHidden)
                        .OrderBy(a => a.SequencingNumber)
                        .ToListAsync();

                    foreach (var item in existingItems)
                    {
                        item.SequencingNumber++;
                        item.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
            else
            {
                // Non-urgent: Append to end
                var maxSequence = await _context.Announcements
                    .Where(a => !a.IsHidden)
                    .MaxAsync(a => (int?)a.SequencingNumber) ?? 0;

                announcement.SequencingNumber = maxSequence + 1;
            }

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created announcement {Id} with sequence number {SequenceNumber}", 
                announcement.Id, announcement.SequencingNumber);

            await _signalRService.NotifyAnnouncementCreatedAsync(announcement);

            return announcement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating announcement");
            throw;
        }
    }

    public async Task<Announcement> UpdateAnnouncementAsync(Announcement announcement)
    {
        try
        {
            announcement.UpdatedAt = DateTime.UtcNow;
            _context.Announcements.Update(announcement);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated announcement {Id}", announcement.Id);

            await _signalRService.NotifyAnnouncementUpdatedAsync(announcement);

            return announcement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating announcement {Id}", announcement.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAnnouncementAsync(Guid id)
    {
        try
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return false;
            }

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted announcement {Id}", id);

            await _signalRService.NotifyAnnouncementDeletedAsync(id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting announcement {Id}", id);
            throw;
        }
    }

    public async Task<Announcement> HideAnnouncementAsync(Guid id)
    {
        try
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                throw new InvalidOperationException($"Announcement {id} not found");
            }

            announcement.IsHidden = true;
            announcement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Hidden announcement {Id}", id);

            await _signalRService.NotifyAnnouncementQueueChangedAsync();

            return announcement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hiding announcement {Id}", id);
            throw;
        }
    }

    public async Task<Announcement> UnhideAnnouncementAsync(Guid id)
    {
        try
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                throw new InvalidOperationException($"Announcement {id} not found");
            }

            announcement.IsHidden = false;
            announcement.UpdatedAt = DateTime.UtcNow;

            // Assign a new sequence number at the end
            var maxSequence = await _context.Announcements
                .Where(a => !a.IsHidden)
                .MaxAsync(a => (int?)a.SequencingNumber) ?? 0;

            announcement.SequencingNumber = maxSequence + 1;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Unhidden announcement {Id}", id);

            await _signalRService.NotifyAnnouncementQueueChangedAsync();

            return announcement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unhiding announcement {Id}", id);
            throw;
        }
    }

    public async Task<Announcement> CallAnnouncementAsync(Guid id)
    {
        try
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                throw new InvalidOperationException($"Announcement {id} not found");
            }

            // Log to history
            var historyLog = new AnnouncementHistoryLog
            {
                AnnouncementId = id,
                Timestamp = DateTime.UtcNow
            };
            _context.AnnouncementHistoryLogs.Add(historyLog);

            // Update announcement
            if (announcement.FirstAnnouncementTime == null)
            {
                announcement.FirstAnnouncementTime = DateTime.UtcNow;
            }
            announcement.LastAnnouncementTime = DateTime.UtcNow;
            announcement.AnnouncedCount++;
            announcement.RemainingRepeatCount--;
            announcement.UpdatedAt = DateTime.UtcNow;

            // Check if repeats remain
            if (announcement.RemainingRepeatCount > 0)
            {
                // Move to end of queue
                var maxSequence = await _context.Announcements
                    .Where(a => !a.IsHidden && a.Id != id)
                    .MaxAsync(a => (int?)a.SequencingNumber) ?? 0;

                announcement.SequencingNumber = maxSequence + 1;

                _logger.LogInformation("Called announcement {Id}, moved to end with sequence {SequenceNumber}. Repeats remaining: {RemainingRepeatCount}",
                    id, announcement.SequencingNumber, announcement.RemainingRepeatCount);
            }
            else
            {
                // No repeats left - hide it
                announcement.IsHidden = true;

                _logger.LogInformation("Called announcement {Id}, marked as complete and hidden", id);
            }

            await _context.SaveChangesAsync();

            await _signalRService.NotifyAnnouncementCalledAsync(announcement);

            return announcement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling announcement {Id}", id);
            throw;
        }
    }

    public async Task<Announcement> DeferAnnouncementAsync(Guid id)
    {
        try
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                throw new InvalidOperationException($"Announcement {id} not found");
            }

            // Move to end of queue
            var maxSequence = await _context.Announcements
                .Where(a => !a.IsHidden && a.Id != id)
                .MaxAsync(a => (int?)a.SequencingNumber) ?? 0;

            announcement.SequencingNumber = maxSequence + 1;
            announcement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deferred announcement {Id} to sequence {SequenceNumber}", 
                id, announcement.SequencingNumber);

            await _signalRService.NotifyAnnouncementQueueChangedAsync();

            return announcement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deferring announcement {Id}", id);
            throw;
        }
    }

    public async Task<Announcement> ReannounceAsync(Guid id)
    {
        try
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                throw new InvalidOperationException($"Announcement {id} not found");
            }

            // Reset announcement state (but keep history)
            announcement.RemainingRepeatCount = 1; // Or could use original value if stored
            announcement.IsHidden = false;
            announcement.UpdatedAt = DateTime.UtcNow;

            // Treat as new announcement for sequencing
            if (announcement.Priority == AnnouncementPriority.Urgent)
            {
                // Insert after last non-hidden Urgent item
                var lastUrgent = await _context.Announcements
                    .Where(a => !a.IsHidden && a.Priority == AnnouncementPriority.Urgent && a.Id != id)
                    .OrderByDescending(a => a.SequencingNumber)
                    .FirstOrDefaultAsync();

                if (lastUrgent != null)
                {
                    announcement.SequencingNumber = lastUrgent.SequencingNumber + 1;

                    // Re-sequence all items after this position
                    var itemsToResequence = await _context.Announcements
                        .Where(a => !a.IsHidden && a.SequencingNumber >= announcement.SequencingNumber && a.Id != id)
                        .ToListAsync();

                    foreach (var item in itemsToResequence)
                    {
                        item.SequencingNumber++;
                        item.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    // First urgent item
                    announcement.SequencingNumber = 1;

                    // Re-sequence all existing items
                    var existingItems = await _context.Announcements
                        .Where(a => !a.IsHidden && a.Id != id)
                        .OrderBy(a => a.SequencingNumber)
                        .ToListAsync();

                    foreach (var item in existingItems)
                    {
                        item.SequencingNumber++;
                        item.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
            else
            {
                // Non-urgent: Append to end
                var maxSequence = await _context.Announcements
                    .Where(a => !a.IsHidden && a.Id != id)
                    .MaxAsync(a => (int?)a.SequencingNumber) ?? 0;

                announcement.SequencingNumber = maxSequence + 1;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reannounced announcement {Id} with sequence {SequenceNumber}", 
                id, announcement.SequencingNumber);

            await _signalRService.NotifyAnnouncementQueueChangedAsync();

            return announcement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reannouncing announcement {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<AnnouncementHistoryLog>> GetHistoryForAnnouncementAsync(Guid id)
    {
        try
        {
            return await _context.AnnouncementHistoryLogs
                .Where(h => h.AnnouncementId == id)
                .OrderByDescending(h => h.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving history for announcement {Id}", id);
            throw;
        }
    }
}
