using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Workers;

/// <summary>
/// Thread-safe channel for queuing scoring events to be processed by the automation worker
/// </summary>
public class ScoringChannel
{
    private readonly Channel<ScoringEvent> _channel;
    private readonly ILogger<ScoringChannel> _logger;

    public ScoringChannel(ILogger<ScoringChannel> logger)
    {
        _logger = logger;
        
        // Create a bounded channel with capacity of 1000 events
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        
        _channel = Channel.CreateBounded<ScoringEvent>(options);
    }

    /// <summary>
    /// Gets the channel reader for consuming events
    /// </summary>
    public ChannelReader<ScoringEvent> Reader => _channel.Reader;

    /// <summary>
    /// Queue a call to court event
    /// </summary>
    public async Task QueueCallToCourtAsync(Guid matchId, string source = "scorer")
    {
        var scoringEvent = new ScoringEvent
        {
            EventType = ScoringEventType.CallToCourt,
            MatchId = matchId,
            Source = source,
            UserName = source
        };

        await QueueEventAsync(scoringEvent);
    }

    /// <summary>
    /// Queue a call to support event
    /// </summary>
    public async Task QueueCallToSupportAsync(Guid matchId, string source = "scorer")
    {
        var scoringEvent = new ScoringEvent
        {
            EventType = ScoringEventType.CallToSupport,
            MatchId = matchId,
            Source = source,
            UserName = source
        };

        await QueueEventAsync(scoringEvent);
    }

    /// <summary>
    /// Queue a match start event
    /// </summary>
    public async Task QueueMatchStartAsync(Guid matchId, string source = "scorer")
    {
        var scoringEvent = new ScoringEvent
        {
            EventType = ScoringEventType.MatchStart,
            MatchId = matchId,
            Source = source,
            UserName = source
        };

        await QueueEventAsync(scoringEvent);
    }

    /// <summary>
    /// Queue a match set start event
    /// </summary>
    public async Task QueueMatchSetStartAsync(Guid matchId, int setNumber, string source = "scorer")
    {
        var scoringEvent = new ScoringEvent
        {
            EventType = ScoringEventType.MatchSetStart,
            MatchId = matchId,
            SetNumber = setNumber,
            Source = source,
            UserName = source
        };

        await QueueEventAsync(scoringEvent);
    }

    /// <summary>
    /// Queue a match set end event
    /// </summary>
    public async Task QueueMatchSetEndAsync(Guid matchId, int setNumber, string source = "scorer")
    {
        var scoringEvent = new ScoringEvent
        {
            EventType = ScoringEventType.MatchSetEnd,
            MatchId = matchId,
            SetNumber = setNumber,
            Source = source,
            UserName = source
        };

        await QueueEventAsync(scoringEvent);
    }

    /// <summary>
    /// Queue a revert to previous set event
    /// </summary>
    public async Task QueueMatchSetRevertToPreviousAsync(Guid matchId, int currentSetNumber, string source = "scorer")
    {
        var scoringEvent = new ScoringEvent
        {
            EventType = ScoringEventType.MatchSetRevertToPrevious,
            MatchId = matchId,
            SetNumber = currentSetNumber,
            Source = source,
            UserName = source
        };

        await QueueEventAsync(scoringEvent);
    }

    /// <summary>
    /// Queue a match set score change event
    /// </summary>
    public async Task QueueMatchSetScoreChangeAsync(Guid matchId, int setNumber, Guid teamId, int scoreChange, string source = "scorer")
    {
        var scoringEvent = new ScoringEvent
        {
            EventType = ScoringEventType.MatchSetScoreChange,
            MatchId = matchId,
            SetNumber = setNumber,
            TeamId = teamId,
            ScoreChange = scoreChange,
            Source = source,
            UserName = source
        };

        await QueueEventAsync(scoringEvent);
    }

    /// <summary>
    /// Queue a match end event
    /// </summary>
    public async Task QueueMatchEndAsync(Guid matchId, string source = "scorer")
    {
        var scoringEvent = new ScoringEvent
        {
            EventType = ScoringEventType.MatchEnd,
            MatchId = matchId,
            Source = source,
            UserName = source
        };

        await QueueEventAsync(scoringEvent);
    }

    /// <summary>
    /// Queue a match disputed event
    /// </summary>
    public async Task QueueMatchDisputedAsync(Guid matchId, bool isDisputed, string source = "scorer")
    {
        var scoringEvent = new ScoringEvent
        {
            EventType = ScoringEventType.MatchDisputed,
            MatchId = matchId,
            Source = source,
            UserName = source,
            AdditionalData = new Dictionary<string, object>
            {
                { "IsDisputed", isDisputed }
            }
        };

        await QueueEventAsync(scoringEvent);
    }

    /// <summary>
    /// Internal method to queue an event to the channel
    /// </summary>
    private async Task QueueEventAsync(ScoringEvent scoringEvent)
    {
        try
        {
            await _channel.Writer.WriteAsync(scoringEvent);
            _logger.LogInformation(
                "Queued scoring event: {EventType} for match {MatchId} from {Source}",
                scoringEvent.EventType,
                scoringEvent.MatchId,
                scoringEvent.Source);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to queue scoring event: {EventType} for match {MatchId}",
                scoringEvent.EventType,
                scoringEvent.MatchId);
            throw;
        }
    }
}
