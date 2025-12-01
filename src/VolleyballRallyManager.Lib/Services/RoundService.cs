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

    public async Task<IEnumerable<Round>> GetAllRoundsAsync()
    {
        try
        {
            return await _context.Rounds
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

    public async Task<Round?> GetRoundByIdAsync(Guid id)
    {
        try
        {
            return await _context.Rounds
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving round with ID {RoundId}", id);
            throw;
        }
    }

    public async Task<Round?> GetRoundWithMatchesAsync(Guid id)
    {
        try
        {
            return await _context.Rounds
                .Include(r => r.Matches)
                    .ThenInclude(m => m.HomeTeam)
                .Include(r => r.Matches)
                    .ThenInclude(m => m.AwayTeam)
                .Include(r => r.Matches)
                    .ThenInclude(m => m.Division)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving round with matches for ID {RoundId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Round>> GetRoundsWithMatchesAsync()
    {
        try
        {
            return await _context.Rounds
                .Include(r => r.Matches)
                    .ThenInclude(m => m.HomeTeam)
                .Include(r => r.Matches)
                    .ThenInclude(m => m.AwayTeam)
                .Include(r => r.Matches)
                    .ThenInclude(m => m.Division)
                .OrderBy(r => r.Sequence)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rounds with matches");
            throw;
        }
    }

    public async Task CreateRoundAsync(Round round)
    {
        try
        {
            _context.Rounds.Add(round);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created round: {RoundName} (ID: {RoundId})", round.Name, round.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating round: {RoundName}", round.Name);
            throw;
        }
    }

    public async Task UpdateRoundAsync(Round round)
    {
        try
        {
            _context.Rounds.Update(round);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated round: {RoundName} (ID: {RoundId})", round.Name, round.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating round with ID {RoundId}", round.Id);
            throw;
        }
    }

    public async Task DeleteRoundAsync(Guid id)
    {
        try
        {
            var round = await _context.Rounds.FindAsync(id);
            if (round != null)
            {
                _context.Rounds.Remove(round);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted round with ID {RoundId}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting round with ID {RoundId}", id);
            throw;
        }
    }

    public async Task<int> GetMatchCountForRoundAsync(Guid roundId)
    {
        try
        {
            return await _context.Matches
                .Where(m => m.RoundId == roundId)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting match count for round {RoundId}", roundId);
            throw;
        }
    }

    public async Task<int> GetCompletedMatchCountForRoundAsync(Guid roundId)
    {
        try
        {
            return await _context.Matches
                .Where(m => m.RoundId == roundId && m.IsFinished)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completed match count for round {RoundId}", roundId);
            throw;
        }
    }
}
