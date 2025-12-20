using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public class MatchService : IMatchService
{
    //TODO add logging to methods
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
            .Include(m => m.Division)
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
            CreatedBy = userId,
            UpdateType = UpdateType.ScoreUpdate,
            Content = $"Score updated to {homeTeamScore}:{awayTeamScore}",
            PreviousValue = $"{match.HomeTeamScore}:{match.AwayTeamScore}",
            NewValue = $"{homeTeamScore}:{awayTeamScore}"
        };

        await AddMatchUpdateAsync(update);
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
            match.ActualStartTime = DateTime.Now;
            var update = new MatchUpdate
            {
                MatchId = id,
                CreatedBy = userName,
                UpdateType = UpdateType.MatchStarted,
                Content = $"Match started"
            };
            match.CurrentSetNumber = 1;
            await _context.SaveChangesAsync();
            await AddMatchUpdateAsync(update);
        }
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
            CreatedBy = userId,
            UpdateType = UpdateType.MatchFinished,
            Content = $"Match finished by {userId}"
        };
        await AddMatchUpdateAsync(update);
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
            CreatedBy = userId,
            UpdateType = UpdateType.DisputeRaised,
            Content = $"Match disputed by {userId}"
        };
        await AddMatchUpdateAsync(update);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchDisputedAsync(match);
        return match;
    }

    public async Task<Match> AssignRefereeAsync(Guid id, string refereeName, string userName)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        match.RefereeName = refereeName;
        var update = new MatchUpdate
        {
            MatchId = id,
            CreatedBy = userName,
            UpdateType = UpdateType.RefereeAssigned,
            Content = $"Referee assigned: {refereeName}"
        };

        await AddMatchUpdateAsync(update);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchUpdatedAsync(match);
        return match;
    }

    public async Task<Match> AssignScorerAsync(Guid id, string scorerName, string userName)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        match.ScorerName = scorerName;
        var update = new MatchUpdate
        {
            MatchId = id,
            CreatedBy = userName,
            UpdateType = UpdateType.ScorerAssigned,
            Content = $"Scorer assigned: {scorerName}"
        };

        await AddMatchUpdateAsync(update);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchUpdatedAsync(match);
        return match;
    }

    public async Task<Match> UpdateLocationAsync(Guid id, string location, string userName)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        var oldLocation = match.CourtLocation;
        match.CourtLocation = location;
        var update = new MatchUpdate
        {
            MatchId = id,
            CreatedBy = userName,
            UpdateType = UpdateType.LocationChanged,
            Content = $"Location changed from {oldLocation} to {location}",
            PreviousValue = oldLocation,
            NewValue = location
        };

        await AddMatchUpdateAsync(update);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyMatchUpdatedAsync(match);
        return match;
    }

    public async Task<Match> UpdateTimeAsync(Guid id, DateTime scheduledTime, string userName)
    {
        var match = await GetMatchAsync(id);
        if (match == null) throw new KeyNotFoundException("Match not found");

        var oldTime = match.ScheduledTime;
        match.ScheduledTime = scheduledTime;
        var update = new MatchUpdate
        {
            MatchId = id,
            CreatedBy = userName,
            UpdateType = UpdateType.TimeChanged,
            Content = $"Time changed from {oldTime:g} to {scheduledTime:g}",
            PreviousValue = oldTime.ToString("g"),
            NewValue = scheduledTime.ToString("g")
        };
        await AddMatchUpdateAsync(update);
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
        CreateBaseEntity(update, update.CreatedBy);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyAddFeedAsync(update);
        return update;
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
            matchSet.UpdatedAt = DateTime.Now;
            matchSet.UpdatedBy = userId;
        }

        var match = await GetMatchAsync(matchId);
        if (match != null)
        {
            var update = new MatchUpdate
            {
                MatchId = matchId,
                CreatedBy = userId,
                UpdateType = UpdateType.ScoreUpdate,
                Content = $"Set {setNumber} score updated to {homeScore}-{awayScore}"
            };
            await AddMatchUpdateAsync(update);
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
        matchSet.UpdatedAt = DateTime.Now;
        matchSet.UpdatedBy = userId;
        await _context.SaveChangesAsync();

        var update = new MatchUpdate
        {
            MatchId = matchId,
            CreatedBy = userId,
            UpdateType = UpdateType.MatchSetFinished,
            Content = $"Set {setNumber} finished: {matchSet.HomeTeamScore}-{matchSet.AwayTeamScore}"
        };
        await AddMatchUpdateAsync(update);


        var match = await GetMatchAsync(matchId);
        if (match != null)
        {
            match.HomeTeamScore = _context.MatchSets.Where(s => s.MatchId == match.Id && s.HomeTeamScore > s.AwayTeamScore).Count();
            match.AwayTeamScore = _context.MatchSets.Where(s => s.MatchId == match.Id && s.HomeTeamScore < s.AwayTeamScore).Count(); ;

            await _context.SaveChangesAsync();
            await _notificationService.NotifyMatchUpdatedAsync(match);
        }
        await _context.SaveChangesAsync();
        // Match sets won/lost are already updated above (lines 452-453)
        return matchSet;
    }

    public async Task<MatchSet> StartNextSetAsync(Guid matchId, string userName, int currentSetNumber)
    {
        var match = await _context.Matches.FindAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException("Match not found");

        match.CurrentSetNumber++;

        var newSet = await GetOrCreateMatchSetAsync(matchId, match.CurrentSetNumber, userName);
        var update = new MatchUpdate
        {
            MatchId = matchId,
            CreatedBy = userName,
            UpdateType = UpdateType.MatchSetStarted,
            Content = $"Set {match.CurrentSetNumber} started"
        };
        await AddMatchUpdateAsync(update);

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
            previousSet.UpdatedAt = DateTime.Now;
            previousSet.UpdatedBy = userId;
        }

        if (currentSet != null)
        {
            _context.MatchSets.Remove(currentSet);
        }

        var update = new MatchUpdate
        {
            MatchId = matchId,
            CreatedBy = userId,
            UpdateType = UpdateType.Other,
            Content = $"Reverted to Set {match.CurrentSetNumber}"
        };
        await AddMatchUpdateAsync(update);

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

        // Lock all sets
        foreach (var set in sets)
        {
            set.IsLocked = true;
            set.IsFinished = true;
        }
        await _context.SaveChangesAsync();

        return match;
    }

    public async Task<Match> UpdateMatchDetailsAsync(Guid matchId, DateTime? scheduledTime, string? courtLocation, string? refereeName, string? scorerName, string userId)
    {
        int typeCount = 0;
        UpdateType updateType = UpdateType.Other;
        var match = await GetMatchAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException("Match not found");

        var changes = new List<string>();
        bool scorerChanged = false;
        bool refereeChanged = false;
        if (scheduledTime.HasValue && match.ScheduledTime != scheduledTime.Value)
        {
            var oldTime = match.ScheduledTime;
            match.ScheduledTime = scheduledTime.Value;
            changes.Add($"Time changed from {oldTime:g} to {scheduledTime.Value:g}");
            updateType = UpdateType.TimeChanged;
        }

        if (!string.IsNullOrEmpty(courtLocation) && match.CourtLocation != courtLocation)
        {
            var oldLocation = match.CourtLocation;
            match.CourtLocation = courtLocation;
            changes.Add($"Court changed from {oldLocation} to {courtLocation}");
            updateType = UpdateType.LocationChanged;
        }

        if (refereeName != null && match.RefereeName != refereeName)
        {
            match.RefereeName = refereeName;
            changes.Add($"Referee assigned: {refereeName}");
            refereeChanged = true;
            updateType = UpdateType.RefereeAssigned;
        }

        if (scorerName != null && match.ScorerName != scorerName)
        {
            match.ScorerName = scorerName;
            changes.Add($"Scorer assigned: {scorerName}");
            scorerChanged = true;
            updateType = UpdateType.ScorerAssigned;
        }

        if (changes.Count > 0)
        {
            var update = new MatchUpdate
            {
                MatchId = matchId,
                CreatedBy = userId,
                UpdateType = UpdateType.Other,
                Content = string.Join("; ", changes)
            };
            if (changes.Count == 1)
            {
                update.UpdateType = updateType;
            }
            if (changes.Count == 2 && scorerChanged && refereeChanged)
            {
                update.UpdateType = UpdateType.OfficialAssigned;
            }
            await AddMatchUpdateAsync(update);

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

    public async Task<bool> IsCalledToCourt(Guid matchId)
    {
        return await _context.MatchUpdates.AnyAsync(mu => mu.MatchId == matchId && mu.UpdateType == UpdateType.CalledToCourt);
    }

    public async Task<List<MatchUpdate>> GetRecentMatchUpdatesAsync(int count = 25)
    {
        return await _context.MatchUpdates
            .Include(u => u.Match)
                .ThenInclude(m => m.HomeTeam)
            .Include(u => u.Match)
                .ThenInclude(m => m.AwayTeam)
            .Include(u => u.Match)
                .ThenInclude(m => m.Round)
            .OrderByDescending(u => u.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> DeleteAllMatchesByTournamentAsync(Guid tournamentId)
    {
        // Get all matches for these rounds
        var matches = await _context.Matches
            .Where(m => m.TournamentId == tournamentId)
            .ToListAsync();

        if (!matches.Any())
        {
            return 0;
        }

        var matchIds = matches.Select(m => m.Id).ToList();

        // Delete all match sets for these matches
        var matchSets = await _context.MatchSets
            .Where(ms => matchIds.Contains(ms.MatchId))
            .ToListAsync();
        _context.MatchSets.RemoveRange(matchSets);

        // Delete all match updates for these matches
        var matchUpdates = await _context.MatchUpdates
            .Where(mu => matchIds.Contains(mu.MatchId))
            .ToListAsync();
        _context.MatchUpdates.RemoveRange(matchUpdates);

        // Delete all matches
        _context.Matches.RemoveRange(matches);

        await _context.SaveChangesAsync();

        return matches.Count();
    }
}
