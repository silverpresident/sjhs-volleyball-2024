using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Common;

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
