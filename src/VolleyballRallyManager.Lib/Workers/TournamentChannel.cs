using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Common;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Workers;

/// <summary>
/// Thread-safe channel for queuing Tournament events to be processed by the automation worker
/// </summary>
public class TournamentChannel
{
    private readonly Channel<TournamentEvent> _channel;
    private readonly ILogger<TournamentChannel> _logger;

    public TournamentChannel(ILogger<TournamentChannel> logger)
    {
        _logger = logger;
        
        // Create a bounded channel with capacity of 1000 events
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        
        _channel = Channel.CreateBounded<TournamentEvent>(options);
    }

    /// <summary>
    /// Gets the channel reader for consuming events
    /// </summary>
    public ChannelReader<TournamentEvent> Reader => _channel.Reader;

    public async Task UpdateDivisionRanksAsync(Guid tournamentId, Guid divisionId, string source = "admin")
    {
        var TournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.UpdateDivisionRanks,
            TournamentId = tournamentId,
            DivisionId = divisionId,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(TournamentEvent);
    }

    #region SignalR Notification Methods

    /// <summary>
    /// Notify clients that a match has been created
    /// </summary>
    public async Task NotifyMatchCreatedAsync(Match match, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.MatchCreated,
            TournamentId = match.TournamentId,
            MatchId = match.Id,
            Match = match,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that a match has been updated
    /// </summary>
    public async Task NotifyMatchUpdatedAsync(Match match, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.MatchUpdated,
            TournamentId = match.TournamentId,
            MatchId = match.Id,
            Match = match,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that a match has started
    /// </summary>
    public async Task NotifyMatchStartedAsync(Match match, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.MatchStarted,
            TournamentId = match.TournamentId,
            MatchId = match.Id,
            Match = match,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that a match has finished
    /// </summary>
    public async Task NotifyMatchFinishedAsync(Match match, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.MatchFinished,
            TournamentId = match.TournamentId,
            MatchId = match.Id,
            Match = match,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that a match has been disputed
    /// </summary>
    public async Task NotifyMatchDisputedAsync(Match match, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.MatchDisputedNotification,
            TournamentId = match.TournamentId,
            MatchId = match.Id,
            Match = match,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients of a score update
    /// </summary>
    public async Task NotifyScoreUpdateAsync(Match match, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.ScoreUpdate,
            TournamentId = match.TournamentId,
            MatchId = match.Id,
            Match = match,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that a bulletin has been created
    /// </summary>
    public async Task NotifyBulletinCreatedAsync(Bulletin bulletin, Guid tournamentId, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.BulletinCreated,
            TournamentId = tournamentId,
            EntityId = bulletin.Id,
            Bulletin = bulletin,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that a bulletin has been updated
    /// </summary>
    public async Task NotifyBulletinUpdatedAsync(Bulletin bulletin, Guid tournamentId, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.BulletinUpdated,
            TournamentId = tournamentId,
            EntityId = bulletin.Id,
            Bulletin = bulletin,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that a bulletin has been deleted
    /// </summary>
    public async Task NotifyBulletinDeletedAsync(Guid bulletinId, Guid tournamentId, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.BulletinDeleted,
            TournamentId = tournamentId,
            EntityId = bulletinId,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that a team has been created
    /// </summary>
    public async Task NotifyTeamCreatedAsync(Team team, Guid tournamentId, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.TeamCreated,
            TournamentId = tournamentId,
            TeamId = team.Id,
            Team = team,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that a team has been updated
    /// </summary>
    public async Task NotifyTeamUpdatedAsync(Team team, Guid tournamentId, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.TeamUpdated,
            TournamentId = tournamentId,
            TeamId = team.Id,
            Team = team,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that a team has been deleted
    /// </summary>
    public async Task NotifyTeamDeletedAsync(Guid teamId, Guid tournamentId, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.TeamDeleted,
            TournamentId = tournamentId,
            TeamId = teamId,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients of a tournament status change
    /// </summary>
    public async Task NotifyTournamentStatusAsync(string status, Guid tournamentId, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.TournamentStatus,
            TournamentId = tournamentId,
            Message = status,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients of an error
    /// </summary>
    public async Task NotifyErrorAsync(string error, Guid tournamentId, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.ErrorNotification,
            TournamentId = tournamentId,
            Message = error,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Broadcast a general message to all clients
    /// </summary>
    public async Task BroadcastMessageAsync(string message, Guid tournamentId, string type = "info", string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.BroadcastMessage,
            TournamentId = tournamentId,
            Message = message,
            MessageType = type,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients of a new match update in the feed
    /// </summary>
    public async Task NotifyAddFeedAsync(MatchUpdate update, Guid tournamentId, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.AddFeed,
            TournamentId = tournamentId,
            MatchId = update.MatchId,
            MatchUpdate = update,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that an announcement has been created
    /// </summary>
    public async Task NotifyAnnouncementCreatedAsync(Announcement announcement, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.AnnouncementCreated,
            TournamentId = announcement.TournamentId,
            EntityId = announcement.Id,
            Announcement = announcement,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that an announcement has been updated
    /// </summary>
    public async Task NotifyAnnouncementUpdatedAsync(Announcement announcement, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.AnnouncementUpdated,
            TournamentId = announcement.TournamentId,
            EntityId = announcement.Id,
            Announcement = announcement,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that an announcement has been deleted
    /// </summary>
    public async Task NotifyAnnouncementDeletedAsync(Guid announcementId, Guid tournamentId, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.AnnouncementDeleted,
            TournamentId = tournamentId,
            EntityId = announcementId,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that an announcement property has changed
    /// </summary>
    public async Task NotifyAnnouncementPropertyChangedAsync(Guid announcementId, string property, string value, Guid tournamentId, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.AnnouncementPropertyChanged,
            TournamentId = tournamentId,
            EntityId = announcementId,
            Property = property,
            Value = value,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that the announcement queue has changed
    /// </summary>
    public async Task NotifyAnnouncementQueueChangedAsync(List<Announcement> announcements, Guid tournamentId, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.AnnouncementQueueChanged,
            TournamentId = tournamentId,
            Announcements = announcements,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    /// <summary>
    /// Notify clients that an announcement has been called
    /// </summary>
    public async Task NotifyAnnouncementCalledAsync(Announcement announcement, string source = "system")
    {
        var tournamentEvent = new TournamentEvent
        {
            EventType = TournamentEventType.AnnouncementCalled,
            TournamentId = announcement.TournamentId,
            EntityId = announcement.Id,
            Announcement = announcement,
            Source = source,
            UserName = source
        };
        await QueueEventAsync(tournamentEvent);
    }

    #endregion

    /*
/// <summary>
/// Queue a call to court event
/// </summary>
public async Task QueueCallToCourtAsync(Guid matchId, string source = "scorer")
{
   var TournamentEvent = new TournamentEvent
   {
       EventType = TournamentEventType.CallToCourt,
       MatchId = matchId,
       Source = source,
       UserName = source
   };

   await QueueEventAsync(TournamentEvent);
}

/// <summary>
/// Queue a call to support event
/// </summary>
public async Task QueueCallToSupportAsync(Guid matchId, string source = "scorer")
{
   var TournamentEvent = new TournamentEvent
   {
       EventType = TournamentEventType.CallToSupport,
       MatchId = matchId,
       Source = source,
       UserName = source
   };

   await QueueEventAsync(TournamentEvent);
}

/// <summary>
/// Queue a match start event
/// </summary>
public async Task QueueMatchStartAsync(Guid matchId, string source = "scorer")
{
   var TournamentEvent = new TournamentEvent
   {
       EventType = TournamentEventType.MatchStart,
       MatchId = matchId,
       Source = source,
       UserName = source
   };

   await QueueEventAsync(TournamentEvent);
}

/// <summary>
/// Queue a match set start event
/// </summary>
public async Task QueueMatchSetStartAsync(Guid matchId, int setNumber, string source = "scorer")
{
   var TournamentEvent = new TournamentEvent
   {
       EventType = TournamentEventType.MatchSetStart,
       MatchId = matchId,
       SetNumber = setNumber,
       Source = source,
       UserName = source
   };

   await QueueEventAsync(TournamentEvent);
}

/// <summary>
/// Queue a match set end event
/// </summary>
public async Task QueueMatchSetEndAsync(Guid matchId, int setNumber, string source = "scorer")
{
   var TournamentEvent = new TournamentEvent
   {
       EventType = TournamentEventType.MatchSetEnd,
       MatchId = matchId,
       SetNumber = setNumber,
       Source = source,
       UserName = source
   };

   await QueueEventAsync(TournamentEvent);
}

/// <summary>
/// Queue a revert to previous set event
/// </summary>
public async Task QueueMatchSetRevertToPreviousAsync(Guid matchId, int currentSetNumber, string source = "scorer")
{
   var TournamentEvent = new TournamentEvent
   {
       EventType = TournamentEventType.MatchSetRevertToPrevious,
       MatchId = matchId,
       SetNumber = currentSetNumber,
       Source = source,
       UserName = source
   };

   await QueueEventAsync(TournamentEvent);
}

/// <summary>
/// Queue a match set score change event
/// </summary>
public async Task QueueMatchSetScoreChangeAsync(Guid matchId, int setNumber, Guid teamId, int scoreChange, string source = "scorer")
{
   var TournamentEvent = new TournamentEvent
   {
       EventType = TournamentEventType.MatchSetScoreChange,
       MatchId = matchId,
       SetNumber = setNumber,
       TeamId = teamId,
       ScoreChange = scoreChange,
       Source = source,
       UserName = source
   };

   await QueueEventAsync(TournamentEvent);
}

/// <summary>
/// Queue a match end event
/// </summary>
public async Task QueueMatchEndAsync(Guid matchId, string source = "scorer")
{
   var TournamentEvent = new TournamentEvent
   {
       EventType = TournamentEventType.MatchEnd,
       MatchId = matchId,
       Source = source,
       UserName = source
   };

   await QueueEventAsync(TournamentEvent);
}

/// <summary>
/// Queue a match disputed event
/// </summary>
public async Task QueueMatchDisputedAsync(Guid matchId, bool isDisputed, string source = "scorer")
{
   var TournamentEvent = new TournamentEvent
   {
       EventType = TournamentEventType.MatchDisputed,
       MatchId = matchId,
       Source = source,
       UserName = source,
       AdditionalData = new Dictionary<string, object>
       {
           { "IsDisputed", isDisputed }
       }
   };

   await QueueEventAsync(TournamentEvent);
}
*/
    /// <summary>
    /// Internal method to queue an event to the channel
    /// </summary>
    private async Task QueueEventAsync(TournamentEvent TournamentEvent)
    {
        try
        {
            await _channel.Writer.WriteAsync(TournamentEvent);
            _logger.LogInformation(
                "Queued Tournament event: {EventType} for Tournament {TournamentId} from {Source}",
                TournamentEvent.EventType,
                TournamentEvent.TournamentId,
                TournamentEvent.Source);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to queue Tournament event: {EventType} for Tournament {TournamentId}",
                TournamentEvent.EventType,
                TournamentEvent.TournamentId);
            throw;
        }
    }
}
