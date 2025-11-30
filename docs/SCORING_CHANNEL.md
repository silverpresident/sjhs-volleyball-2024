# Scoring Channel Implementation

## Overview

The scoring channel implementation provides a robust, queue-based system for handling scoring events in the volleyball tournament management system. This architecture prevents race conditions and ensures proper sequencing of scoring actions.

## Architecture

### Components

1. **ScoringEventType** (Enum)
   - Defines all possible scoring event types
   - Located in `Models/ScoringEventType.cs`

2. **ScoringEvent** (Model)
   - Represents a scoring event with all necessary data
   - Located in `Models/ScoringEvent.cs`
   - Contains: EventType, MatchId, SetNumber, TeamId, ScoreChange, AdditionalData, Timestamp, Source

3. **ScoringChannel**
   - Thread-safe channel for queuing scoring events
   - Located in `Workers/ScoringChannel.cs`
   - Uses `System.Threading.Channels` with bounded capacity of 1000 events
   - Provides typed methods for each event type

4. **ScoringAutomationWorker**
   - Background service that processes events from the channel
   - Located in `Workers/ScoringAutomationWorker.cs`
   - Inherits from `BackgroundService`
   - Processes events sequentially to avoid race conditions

5. **ScorerHub** (Updated)
   - SignalR hub updated to use the ScoringChannel
   - Queues events instead of directly calling services
   - Provides optimistic UI updates for better UX

## Event Types

### CallToCourt
- Calls teams to court for their match
- Creates a match update and public announcement
- **Handler**: `HandleCallToCourtAsync`

### CallToSupport
- Requests admin/support assistance
- Creates a match update
- **Handler**: `HandleCallToSupportAsync`

### MatchStart
- Marks the match as started
- Sets the actual start time
- **Handler**: `HandleMatchStartAsync`

### MatchSetStart
- Starts a new set
- Finishes previous set if needed
- **Handler**: `HandleMatchSetStartAsync`

### MatchSetEnd
- Ends the current set
- Locks the set scores
- **Handler**: `HandleMatchSetEndAsync`

### MatchSetRevertToPrevious
- Reverts to the previous set
- Unlocks and adjusts set numbers
- **Handler**: `HandleMatchSetRevertToPreviousAsync`

### MatchSetScoreChange
- Updates the score for a team in a set
- Validates team, set, and lock status
- **Handler**: `HandleMatchSetScoreChangeAsync`

### MatchEnd
- Ends the match and locks all sets
- Calculates final scores
- **Handler**: `HandleMatchEndAsync`

### MatchDisputed
- Marks or clears a dispute on a match
- Creates a match update
- **Handler**: `HandleMatchDisputedAsync`

## Usage Examples

### Queuing Events from ScorerHub

```csharp
// Score change
await _scoringChannel.QueueMatchSetScoreChangeAsync(matchId, setNumber, teamId, incrementValue, "scorer");

// Match start
await _scoringChannel.QueueMatchStartAsync(matchId, "scorer");

// Call to court
await _scoringChannel.QueueCallToCourtAsync(matchId, "scorer");
```

### Processing in ScoringAutomationWorker

Events are automatically processed by the worker in the order they are queued:

```csharp
await foreach (var scoringEvent in _scoringChannel.Reader.ReadAllAsync(stoppingToken))
{
    await ProcessEventAsync(scoringEvent, stoppingToken);
}
```

## Benefits

### 1. Concurrency Control
- Events are processed sequentially per match
- Prevents race conditions from multiple scorers
- Ensures data consistency

### 2. Reliability
- Failed events don't block other events
- Comprehensive error logging
- Graceful degradation

### 3. Scalability
- Channel can handle bursts of events
- Bounded channel prevents memory issues
- Automatic backpressure handling

### 4. Testability
- Channel can be mocked for unit tests
- Clear separation of concerns
- Easy to test individual handlers

### 5. Monitoring
- Detailed logging for each event
- Easy to add metrics on queue depth
- Processing time tracking capability

## Configuration

The ScoringChannel and ScoringAutomationWorker are registered in `ServiceCollectionExtensions.cs`:

```csharp
// Scoring Channel and Workers
services.AddSingleton<ScoringChannel>();
services.AddHostedService<ScoringAutomationWorker>();
```

## Channel Settings

- **Capacity**: 1000 events
- **Full Mode**: Wait (blocks when full)
- **Lifecycle**: Singleton (shared across requests)

## Worker Settings

- **Lifecycle**: Hosted Service (starts with application)
- **Service Scope**: Creates new scope per event for scoped services
- **Cancellation**: Properly handles shutdown requests

## Error Handling

- Each event handler has try-catch blocks
- Errors are logged with context
- Failed events don't crash the worker
- Worker continues processing subsequent events

## Future Enhancements

1. **Metrics**: Add Application Insights metrics for queue depth and processing time
2. **Priority Queue**: Implement priority handling for critical events
3. **Dead Letter Queue**: Store failed events for retry
4. **Admin Notifications**: Implement real-time admin notifications for CallToSupport
5. **Event Replay**: Add capability to replay events for debugging
6. **Rate Limiting**: Add rate limiting per match to prevent abuse

## Related Files

- `Models/ScoringEventType.cs` - Event type enum
- `Models/ScoringEvent.cs` - Event model
- `Workers/ScoringChannel.cs` - Channel implementation
- `Workers/ScoringAutomationWorker.cs` - Background worker
- `Hubs/ScorerHub.cs` - SignalR hub (updated)
- `Configuration/ServiceCollectionExtensions.cs` - Service registration

## Testing Recommendations

1. **Unit Tests**: Test individual event handlers with mocked services
2. **Integration Tests**: Test full event flow from hub to database
3. **Load Tests**: Test channel capacity under high load
4. **Concurrency Tests**: Verify no race conditions with multiple scorers
5. **Failure Tests**: Verify worker continues after handler failures
