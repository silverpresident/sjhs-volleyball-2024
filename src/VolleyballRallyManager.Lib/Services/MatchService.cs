using Microsoft.AspNetCore.Http.HttpResults;
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
            Content = $"Score updated to {homeTeamScore}:{awayTeamScore}",
            PreviousValue = $"{match.HomeTeamScore}:{match.AwayTeamScore}",
            NewValue = $"{homeTeamScore}:{awayTeamScore}"
        };

        _context.MatchUpdates.Add(update);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyScoreUpdateAsync(match);
        return match;
    }

    public async Task<Match> StartMatchAsync(Guid id, string userName)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        if (match.IsLocked)
        {
            throw new KeyNotFoundException("Match is locked");
        }
        if (match.CurrentSetNumber == 0)
        {
            match.ActualStartTime = DateTime.UtcNow;
            var update = new MatchUpdate
            {
                MatchId = id,
                UpdateType = UpdateType.MatchStarted,
                Content = $"Match started"
            };
            match.CurrentSetNumber = 1;
            CreateBaseEntity(update, userName);
            _context.MatchUpdates.Add(update);
            await _context.SaveChangesAsync();
        }
        await _notificationService.NotifyMatchStartedAsync(match);
        return match;
    }

    private void CreateBaseEntity(BaseEntity update, string userName)
    {
        update.CreatedBy = userName;
        update.UpdatedBy = userName;
        update.CreatedAt = DateTime.Now;
        update.UpdatedAt = DateTime.Now;
    }

    public async Task<Match> FinishMatchAsync(Guid id, string userId)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        match.IsFinished = true;
        match.HomeTeamScore = _context.MatchSets.Where(s => s.MatchId == match.Id && s.HomeTeamScore > s.AwayTeamScore).Count();
        match.AwayTeamScore = _context.MatchSets.Where(s => s.MatchId == match.Id && s.HomeTeamScore < s.AwayTeamScore).Count(); ;


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

    // MatchSet operations
    public async Task<List<MatchSet>> GetMatchSetsAsync(Guid matchId)
    {
        return await _context.MatchSets
            .Where(ms => ms.MatchId == matchId)
            .OrderBy(ms => ms.SetNumber)
            .ToListAsync();
    }

    public async Task<MatchSet?> GetCurrentSetAsync(Guid matchId)
    {
        var match = await _context.Matches.FindAsync(matchId);
        if (match == null) return null;

        return await _context.MatchSets
            .FirstOrDefaultAsync(ms => ms.MatchId == matchId && ms.SetNumber == match.CurrentSetNumber);
    }

    public async Task<MatchSet> UpdateSetScoreAsync(Guid matchId, int setNumber, int homeScore, int awayScore, string userId)
    {
        var matchSet = await _context.MatchSets
            .FirstOrDefaultAsync(ms => ms.MatchId == matchId && ms.SetNumber == setNumber);

        if (matchSet == null)
        {
            // Create new set if it doesn't exist
            matchSet = new MatchSet
            {
                MatchId = matchId,
                SetNumber = setNumber,
                HomeTeamScore = homeScore,
                AwayTeamScore = awayScore
            };
            CreateBaseEntity(matchSet, userId);
            _context.MatchSets.Add(matchSet);
        }
        else
        {
            if (matchSet.IsLocked)
                throw new InvalidOperationException("Cannot update a locked set");

            matchSet.HomeTeamScore = homeScore;
            matchSet.AwayTeamScore = awayScore;
            matchSet.UpdatedAt = DateTime.UtcNow;
            matchSet.UpdatedBy = userId;
        }

        var match = await GetMatchAsync(matchId);
        if (match != null)
        {
            var update = new MatchUpdate
            {
                MatchId = matchId,
                UpdateType = UpdateType.ScoreUpdate,
                Content = $"Set {setNumber} score updated to {homeScore}-{awayScore}"
            };
            _context.MatchUpdates.Add(update);
        }

        await _context.SaveChangesAsync();
        
        if (match != null)
        {
            await _notificationService.NotifyScoreUpdateAsync(match);
        }

        return matchSet;
    }

    public async Task<MatchSet> FinishSetAsync(Guid matchId, int setNumber, string userId)
    {
        var matchSet = await _context.MatchSets
            .FirstOrDefaultAsync(ms => ms.MatchId == matchId && ms.SetNumber == setNumber);

        if (matchSet == null)
            throw new KeyNotFoundException("Set not found");

        if (matchSet.IsFinished == true)
        {
            return matchSet;
        }
        matchSet.IsFinished = true;
        matchSet.UpdatedAt = DateTime.UtcNow;
        matchSet.UpdatedBy = userId;

        var update = new MatchUpdate
        {
            MatchId = matchId,
            UpdateType = UpdateType.MatchSetFinished,
            Content = $"Set {setNumber} finished: {matchSet.HomeTeamScore}-{matchSet.AwayTeamScore}"
        };
        _context.MatchUpdates.Add(update);

        await _context.SaveChangesAsync();

        var match = await GetMatchAsync(matchId);
        if (match != null)
        {
            match.HomeTeamScore = _context.MatchSets.Where(s => s.MatchId == match.Id && s.HomeTeamScore > s.AwayTeamScore).Count();
            match.AwayTeamScore = _context.MatchSets.Where(s => s.MatchId == match.Id && s.HomeTeamScore < s.AwayTeamScore).Count(); ;

            await _context.SaveChangesAsync();
            await _notificationService.NotifyMatchUpdatedAsync(match);
        } 
        //TODO update match with sets won v lost
        return matchSet;
    }

    public async Task<MatchSet> StartNextSetAsync(Guid matchId, string userName, int currentSetNumber)
    {
        var match = await _context.Matches.FindAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException("Match not found");

        match.CurrentSetNumber++;

        var newSet = new MatchSet
        {
            MatchId = matchId,
            SetNumber = match.CurrentSetNumber,
            HomeTeamScore = 0,
            AwayTeamScore = 0,
            IsFinished = false,
            IsLocked = false
        };
        CreateBaseEntity(newSet, userName);
        _context.MatchSets.Add(newSet);

        var update = new MatchUpdate
        {
            MatchId = matchId,
            UpdateType = UpdateType.MatchSetStarted,
            Content = $"Set {match.CurrentSetNumber} started"
        };
        CreateBaseEntity(update, userName);
        _context.MatchUpdates.Add(update);

        await _context.SaveChangesAsync();

        var matchFull = await GetMatchAsync(matchId);
        if (matchFull != null)
        {
            await _notificationService.NotifyMatchUpdatedAsync(matchFull);
        }

        return newSet;
    }

    public async Task<MatchSet> RevertToPreviousSetAsync(Guid matchId, string userId, int currentSetNumber)
    {
        var match = await _context.Matches.FindAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException("Match not found");

        if (match.CurrentSetNumber <= 1)
            throw new InvalidOperationException("Cannot revert from first set");

        var currentSet = await _context.MatchSets
            .FirstOrDefaultAsync(ms => ms.MatchId == matchId && ms.SetNumber == match.CurrentSetNumber);

        if (currentSet != null && (currentSet.HomeTeamScore != 0 || currentSet.AwayTeamScore != 0))
            throw new InvalidOperationException("Can only revert if current set score is 0-0");

        match.CurrentSetNumber--;

        var previousSet = await _context.MatchSets
            .FirstOrDefaultAsync(ms => ms.MatchId == matchId && ms.SetNumber == match.CurrentSetNumber);

        if (previousSet != null)
        {
            previousSet.IsFinished = false;
            previousSet.UpdatedAt = DateTime.UtcNow;
            previousSet.UpdatedBy = userId;
        }

        if (currentSet != null)
        {
            _context.MatchSets.Remove(currentSet);
        }

        var update = new MatchUpdate
        {
            MatchId = matchId,
            UpdateType = UpdateType.Other,
            Content = $"Reverted to Set {match.CurrentSetNumber}"
        };
        _context.MatchUpdates.Add(update);

        await _context.SaveChangesAsync();

        var matchFull = await GetMatchAsync(matchId);
        if (matchFull != null)
        {
            await _notificationService.NotifyMatchUpdatedAsync(matchFull);
        }

        return previousSet ?? throw new KeyNotFoundException("Previous set not found");
    }

    public async Task<Match> EndMatchAndLockSetsAsync(Guid matchId, string userId)
    {
        var match = await GetMatchAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException("Match not found");

        var sets = await GetMatchSetsAsync(matchId);

        // Calculate sets won
        int homeSetsWon = sets.Count(s => s.HomeTeamScore > s.AwayTeamScore);
        int awaySetsWon = sets.Count(s => s.AwayTeamScore > s.HomeTeamScore);

        match.HomeTeamScore = homeSetsWon;
        match.AwayTeamScore = awaySetsWon;
        match.IsFinished = true;
        match.IsLocked = true;

        // Lock all sets
        foreach (var set in sets)
        {
            set.IsLocked = true;
            set.IsFinished = true;
        }

        var update = new MatchUpdate
        {
            MatchId = matchId,
            UpdateType = UpdateType.MatchFinished,
            Content = $"Match ended by {userId}. Final score: {homeSetsWon}-{awaySetsWon} sets"
        };
        _context.MatchUpdates.Add(update);

        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchFinishedAsync(match);

        return match;
    }

    public async Task<Match> UpdateMatchDetailsAsync(Guid matchId, DateTime? scheduledTime, string? courtLocation, string? refereeName, string? scorerName, string userId)
    {
        var match = await GetMatchAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException("Match not found");

        var changes = new List<string>();

        if (scheduledTime.HasValue && match.ScheduledTime != scheduledTime.Value)
        {
            var oldTime = match.ScheduledTime;
            match.ScheduledTime = scheduledTime.Value;
            changes.Add($"Time changed from {oldTime:g} to {scheduledTime.Value:g}");
        }

        if (!string.IsNullOrEmpty(courtLocation) && match.CourtLocation != courtLocation)
        {
            var oldLocation = match.CourtLocation;
            match.CourtLocation = courtLocation;
            changes.Add($"Court changed from {oldLocation} to {courtLocation}");
        }

        if (refereeName != null && match.RefereeName != refereeName)
        {
            match.RefereeName = refereeName;
            changes.Add($"Referee assigned: {refereeName}");
        }

        if (scorerName != null && match.ScorerName != scorerName)
        {
            match.ScorerName = scorerName;
            changes.Add($"Scorer assigned: {scorerName}");
        }

        if (changes.Count > 0)
        {
            var update = new MatchUpdate
            {
                MatchId = matchId,
                UpdateType = UpdateType.Other,
                Content = string.Join("; ", changes)
            };
            _context.MatchUpdates.Add(update);

            await _context.SaveChangesAsync();
            await _notificationService.NotifyMatchUpdatedAsync(match);
        }

        return match;
    }

    public async Task<MatchSet?> GetMatchSetAsync(Guid matchId, int currentSetNumber)
    {
        return await _context.MatchSets
            .FirstOrDefaultAsync(ms => ms.MatchId == matchId && ms.SetNumber == currentSetNumber);

    }
    public async Task<MatchSet> GetOrCreateMatchSetAsync(Guid matchId, int currentSetNumber, string userName)
    {
        var currentSet = await _context.MatchSets
            .FirstOrDefaultAsync(ms => ms.MatchId == matchId && ms.SetNumber == currentSetNumber);
        if (currentSet == null)
        {
            currentSet = new MatchSet()
            {
                MatchId = matchId,
                SetNumber = currentSetNumber,
                HomeTeamScore = 0,
                AwayTeamScore = 0,
                IsLocked = false,
                IsFinished = false,
            };
            CreateBaseEntity(currentSet, userName);
            _context.MatchSets.Add(currentSet);
            await _context.SaveChangesAsync();
        }
        return currentSet;
    }
}
