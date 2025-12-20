using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public class RoundService : IRoundService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RoundService> _logger;

    public RoundService(ApplicationDbContext context, ILogger<RoundService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<RoundTemplate>> GetAllRoundsAsync()
    {
        try
        {
            return await _context.RoundTemplates
                .OrderBy(r => r.Sequence)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all rounds");
            throw;
        }
    }

    public async Task<RoundTemplate?> GetRoundByIdAsync(Guid id)
    {
        try
        {
            return await _context.RoundTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving round with ID {RoundId}", id);
            throw;
        }
    }

    public async Task CreateRoundAsync(RoundTemplate round)
    {
        try
        {
            _context.RoundTemplates.Add(round);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created round: {RoundName} (ID: {RoundId})", round.Name, round.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating round: {RoundName}", round.Name);
            throw;
        }
    }

    public async Task UpdateRoundAsync(RoundTemplate round)
    {
        try
        {
            _context.RoundTemplates.Update(round);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated round: {RoundName} (ID: {RoundId})", round.Name, round.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating round with ID {RoundId}", round.Id);
            throw;
        }
    }


}
