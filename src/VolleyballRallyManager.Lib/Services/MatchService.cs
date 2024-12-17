using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public class MatchService : IMatchService
{
    private readonly ApplicationDbContext _context;
    private readonly ISignalRNotificationService _notificationService;

    public MatchService(ApplicationDbContext context, ISignalRNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Match?> GetMatchAsync(Guid id)
    {
        return await _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Round)
            .Include(m => m.Updates)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<Match>> GetMatchesAsync()
    {
        return await _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Round)
            .OrderBy(m => m.ScheduledTime)
            .ToListAsync();
    }

    public async Task<List<Match>> GetMatchesByRoundAsync(Guid roundId)
    {
        return await _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m => m.RoundId == roundId)
            .OrderBy(m => m.ScheduledTime)
            .ToListAsync();
    }

    public async Task<List<Match>> GetMatchesByTeamAsync(Guid teamId)
    {
        return await _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Round)
            .Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId)
            .OrderBy(m => m.ScheduledTime)
            .ToListAsync();
    }

    public async Task<List<Match>> GetInProgressMatchesAsync()
    {
        return await _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Round)
            .Where(m => m.ActualStartTime.HasValue && !m.IsFinished)
            .OrderBy(m => m.ScheduledTime)
            .ToListAsync();
    }

    public async Task<List<Match>> GetFinishedMatchesAsync()
    {
        return await _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Round)
            .Where(m => m.IsFinished)
            .OrderBy(m => m.ScheduledTime)
            .ToListAsync();
    }

    public async Task<List<Match>> GetDisputedMatchesAsync()
    {
        return await _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Round)
            .Where(m => m.IsDisputed)
            .OrderBy(m => m.ScheduledTime)
            .ToListAsync();
    }

    public async Task<Match> CreateMatchAsync(Match match)
    {
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchCreatedAsync(match);
        return match;
    }

    public async Task<Match> UpdateMatchAsync(Match match)
    {
        _context.Entry(match).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchUpdatedAsync(match);
        return match;
    }

    public async Task<bool> DeleteMatchAsync(Guid id)
    {
        var match = await _context.Matches.FindAsync(id);
        if (match == null) return false;

        _context.Matches.Remove(match);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Match> UpdateMatchScoreAsync(Guid id, int homeTeamScore, int awayTeamScore, string userId)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        match.HomeTeamScore = homeTeamScore;
        match.AwayTeamScore = awayTeamScore;

        var update = new MatchUpdate
        {
            MatchId = id,
            UpdateType = UpdateType.ScoreUpdate,
            Content = $"Score updated to {homeTeamScore}-{awayTeamScore}",
            PreviousValue = $"{match.HomeTeamScore}-{match.AwayTeamScore}",
            NewValue = $"{homeTeamScore}-{awayTeamScore}"
        };

        _context.MatchUpdates.Add(update);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyScoreUpdateAsync(match);
        return match;
    }

    public async Task<Match> StartMatchAsync(Guid id, string userId)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        match.ActualStartTime = DateTime.UtcNow;
        var update = new MatchUpdate
        {
            MatchId = id,
            UpdateType = UpdateType.MatchStarted,
            Content = $"Match started by {userId}"
        };
        _context.MatchUpdates.Add(update);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchStartedAsync(match);
        return match;
    }

    public async Task<Match> FinishMatchAsync(Guid id, string userId)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        match.IsFinished = true;
         var update = new MatchUpdate
        {
            MatchId = id,
            UpdateType = UpdateType.MatchFinished,
            Content = $"Match finished by {userId}"
        };
        _context.MatchUpdates.Add(update);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchFinishedAsync(match);
        return match;
    }

    public async Task<Match> RaiseDisputeAsync(Guid id, string userId)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        match.IsDisputed = true;
        var update = new MatchUpdate
        {
            MatchId = id,
            UpdateType = UpdateType.DisputeRaised,
            Content = $"Match disputed by {userId}"
        };

        _context.MatchUpdates.Add(update);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchDisputedAsync(match);
        return match;
    }

    public async Task<Match> AssignRefereeAsync(Guid id, string refereeName)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        match.RefereeName = refereeName;
        var update = new MatchUpdate
        {
            MatchId = id,
            UpdateType = UpdateType.RefereeAssigned,
            Content = $"Referee assigned: {refereeName}"
        };

        _context.MatchUpdates.Add(update);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchUpdatedAsync(match);
        return match;
    }

    public async Task<Match> AssignScorerAsync(Guid id, string scorerName)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        match.ScorerName = scorerName;
        var update = new MatchUpdate
        {
            MatchId = id,
            UpdateType = UpdateType.ScorerAssigned,
            Content = $"Scorer assigned: {scorerName}"
        };

        _context.MatchUpdates.Add(update);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchUpdatedAsync(match);
        return match;
    }

    public async Task<Match> UpdateLocationAsync(Guid id, string location)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        var oldLocation = match.CourtLocation;
        match.CourtLocation = location;
        var update = new MatchUpdate
        {
            MatchId = id,
            UpdateType = UpdateType.LocationChanged,
            Content = $"Location changed from {oldLocation} to {location}",
            PreviousValue = oldLocation,
            NewValue = location
        };

        _context.MatchUpdates.Add(update);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchUpdatedAsync(match);
        return match;
    }

    public async Task<Match> UpdateTimeAsync(Guid id, DateTime scheduledTime)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        var oldTime = match.ScheduledTime;
        match.ScheduledTime = scheduledTime;
        var update = new MatchUpdate
        {
            MatchId = id,
            UpdateType = UpdateType.TimeChanged,
            Content = $"Time changed from {oldTime:g} to {scheduledTime:g}",
            PreviousValue = oldTime.ToString("g"),
            NewValue = scheduledTime.ToString("g")
        };

        _context.MatchUpdates.Add(update);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchUpdatedAsync(match);
        return match;
    }

    public async Task<List<MatchUpdate>> GetMatchUpdatesAsync(Guid matchId)
    {
        return await _context.MatchUpdates
            .Where(u => u.MatchId == matchId)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<MatchUpdate> AddMatchUpdateAsync(MatchUpdate update)
    {
        _context.MatchUpdates.Add(update);
        await _context.SaveChangesAsync();
        return update;
    }

    public async Task<bool> HasTeamPlayedInRoundAsync(Guid teamId, Guid roundId)
    {
        return await _context.Matches
            .AnyAsync(m => m.RoundId == roundId && (m.HomeTeamId == teamId || m.AwayTeamId == teamId));
    }

    public async Task<bool> AreTeamsAvailableAsync(Guid homeTeamId, Guid awayTeamId, DateTime scheduledTime)
    {
        var timeWindow = TimeSpan.FromHours(2); // Teams can't play within 2 hours of another match
        var startWindow = scheduledTime.Subtract(timeWindow);
        var endWindow = scheduledTime.Add(timeWindow);

        return !await _context.Matches
            .AnyAsync(m => 
                (m.HomeTeamId == homeTeamId || m.AwayTeamId == homeTeamId || 
                 m.HomeTeamId == awayTeamId || m.AwayTeamId == awayTeamId) &&
                m.ScheduledTime >= startWindow && m.ScheduledTime <= endWindow);
    }

    public async Task<bool> IsCourtAvailableAsync(string courtLocation, DateTime scheduledTime)
    {
        var timeWindow = TimeSpan.FromHours(1); // Court must be free for 1 hour between matches
        var startWindow = scheduledTime.Subtract(timeWindow);
        var endWindow = scheduledTime.Add(timeWindow);

        return !await _context.Matches
            .AnyAsync(m => m.CourtLocation == courtLocation &&
                          m.ScheduledTime >= startWindow && m.ScheduledTime <= endWindow);
    }

    public async Task UpdateTeamStatisticsAsync(Guid matchId)
    {
        var match = await GetMatchAsync(matchId);
        if (match == null || !match.IsFinished) return;

        var homeTeam = match.HomeTeam;
        var awayTeam = match.AwayTeam;

        if (homeTeam != null)
        {
            homeTeam.MatchesPlayed++;
            homeTeam.PointsScored += match.HomeTeamScore;
            homeTeam.PointsConceded += match.AwayTeamScore;

            if (match.HomeTeamScore > match.AwayTeamScore)
                homeTeam.Wins++;
            else if (match.HomeTeamScore < match.AwayTeamScore)
                homeTeam.Losses++;
            else
                homeTeam.Draws++;
        }

        if (awayTeam != null)
        {
            awayTeam.MatchesPlayed++;
            awayTeam.PointsScored += match.AwayTeamScore;
            awayTeam.PointsConceded += match.HomeTeamScore;

            if (match.AwayTeamScore > match.HomeTeamScore)
                awayTeam.Wins++;
            else if (match.AwayTeamScore < match.HomeTeamScore)
                awayTeam.Losses++;
            else
                awayTeam.Draws++;
        }

        await _context.SaveChangesAsync();
    }
}
