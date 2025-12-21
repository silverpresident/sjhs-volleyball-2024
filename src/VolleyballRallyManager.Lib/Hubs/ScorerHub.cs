using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;
using VolleyballRallyManager.Lib.Workers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace VolleyballRallyManager.Lib.Hubs;

public class ScorerHub : Hub
{
    private readonly IMatchService _matchService;
    private readonly ScoringChannel _scoringChannel;
    private readonly ILogger<ScorerHub> _logger;

    public ScorerHub(IMatchService matchService, ScoringChannel scoringChannel, ILogger<ScorerHub> logger)
    {
        _matchService = matchService;
        _scoringChannel = scoringChannel;
        _logger = logger;
    }

    public async Task JoinMatchGroup(Guid matchId)
    {
        try
        {
            _logger.LogInformation("Client {ConnectionId} attempting to join scorer group for match {MatchId}", Context.ConnectionId, matchId);
            
            var match = await _matchService.GetMatchAsync(matchId);
            if (match == null)
            {
                _logger.LogWarning("Client {ConnectionId} attempted to join non-existent match {MatchId}", Context.ConnectionId, matchId);
                await Clients.Caller.SendAsync("ReceiveError", "Match not found");
                return;
            }
            
            await Groups.AddToGroupAsync(Context.ConnectionId, $"scorer_{matchId}");
            _logger.LogInformation("Client {ConnectionId} successfully joined scorer_{MatchId} group", Context.ConnectionId, matchId);

            var sets = await _matchService.GetMatchSetsAsync(matchId);
            await Clients.Caller.SendAsync("ReceiveMatchState", new
            {
                MatchId = matchId,
                CurrentSetNumber = match.CurrentSetNumber,
                IsFinished = match.IsFinished,
                IsDisputed = match.IsDisputed,
                IsLocked = match.IsLocked,
                HomeScore = match.HomeTeamScore,
                AwayScore = match.AwayTeamScore,
                HomeSetsWon = match.HomeTeamScore,
                AwaySetsWon = match.AwayTeamScore,
                ActualStartTime = match.ActualStartTime,
                Sets = sets.Select(s => new
                {
                    s.SetNumber,
                    s.HomeTeamScore,
                    s.AwayTeamScore,
                    s.IsFinished,
                    s.IsLocked
                }),
                Timestamp = DateTime.Now
            });
            _logger.LogInformation("Sent initial match state for match {MatchId} to client {ConnectionId}", matchId, Context.ConnectionId);

            var updates = await _matchService.GetMatchUpdatesAsync(matchId);
            var updatesDto = updates.Take(20).Select(u => new
            {
                Content = u.Content,
                CreatedAt = u.CreatedAt,
                Timestamp = u.CreatedAt,
                UpdateType = u.UpdateType
            }).ToList();
            await Clients.Caller.SendAsync("ReceiveFeedList", updatesDto);
            _logger.LogInformation("Sent {Count} match updates for match {MatchId} to client {ConnectionId}", updatesDto.Count, matchId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while client {ConnectionId} joining match {MatchId}", Context.ConnectionId, matchId);
            await Clients.Caller.SendAsync("ReceiveError", "Failed to join match group");
        }
    }

    public async Task LeaveMatchGroup(string matchId)
    {
        _logger.LogInformation("Client {ConnectionId} leaving scorer_{MatchId} group", Context.ConnectionId, matchId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"scorer_{matchId}");
    }

    public async Task SendScoreUpdate(Guid matchId, int setNumber, Guid teamId, int incrementValue)
    {
        try
        {
            _logger.LogInformation("Client {ConnectionId} sending score update for match {MatchId}, set {SetNumber}, team {TeamId}, increment {IncrementValue}",
                Context.ConnectionId, matchId, setNumber, teamId, incrementValue);
            
            var match = await _matchService.GetMatchAsync(matchId);
            if (match == null)
            {
                _logger.LogWarning("Score update failed: Match {MatchId} not found", matchId);
                await Clients.Caller.SendAsync("ReceiveError", "Match not found");
                return;
            }

            var currentSet = await _matchService.GetOrCreateMatchSetAsync(matchId, setNumber, "scorer");

            if (currentSet == null || currentSet.SetNumber != setNumber)
            {
                _logger.LogWarning("Score update failed: Set {SetNumber} not found or not current for match {MatchId}", setNumber, matchId);
                await Clients.Caller.SendAsync("ReceiveError", "Set not found or not current");
                return;
            }

            if (currentSet.IsLocked)
            {
                _logger.LogWarning("Score update failed: Set {SetNumber} is locked for match {MatchId}", setNumber, matchId);
                await Clients.Caller.SendAsync("ReceiveError", "Set is locked");
                return;
            }

            // Queue the score change event
            await _scoringChannel.QueueMatchSetScoreChangeAsync(matchId, setNumber, teamId, incrementValue, "scorer");
            _logger.LogInformation("Queued score change for match {MatchId}, set {SetNumber}", matchId, setNumber);

            // Calculate new score for immediate UI feedback
            int newHomeScore = currentSet.HomeTeamScore;
            int newAwayScore = currentSet.AwayTeamScore;

            if (teamId == match.HomeTeamId)
            {
                newHomeScore += incrementValue;
                if (newHomeScore < 0) newHomeScore = 0;
            }
            else if (teamId == match.AwayTeamId)
            {
                newAwayScore += incrementValue;
                if (newAwayScore < 0) newAwayScore = 0;
            }

            // Broadcast optimistic update (actual update will come from worker)
            await BroadcastScoreUpdate(matchId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing score update for match {MatchId}, set {SetNumber}", matchId, setNumber);
            await Clients.Caller.SendAsync("ReceiveError", ex.Message);
        }
    }

    public async Task SendQuickAction(Guid matchId, string actionType)
    {
        try
        {
            _logger.LogInformation("Client {ConnectionId} sending quick action {ActionType} for match {MatchId}", Context.ConnectionId, actionType, matchId);
            
            var match = await _matchService.GetMatchAsync(matchId);
            if (match == null)
            {
                _logger.LogWarning("Quick action failed: Match {MatchId} not found", matchId);
                await Clients.Caller.SendAsync("ReceiveError", "Match not found");
                return;
            }

            MatchUpdate? update = null;

            switch (actionType.ToLower())
            {
                case "calltosupport":
                    await _scoringChannel.QueueCallToSupportAsync(matchId, "scorer");
                    _logger.LogInformation("Queued call to support for match {MatchId}", matchId);
                    break;

                case "calltocourt":
                    await _scoringChannel.QueueCallToCourtAsync(matchId, "scorer");
                    _logger.LogInformation("Queued call to court for match {MatchId}", matchId);
                    break;

                case "matchstarted":
                    await _scoringChannel.QueueMatchStartAsync(matchId, "scorer");
                    _logger.LogInformation("Queued match start for match {MatchId}", matchId);
                    break;

                case "matchended":
                    await _scoringChannel.QueueMatchEndAsync(matchId, "scorer");
                    _logger.LogInformation("Queued match end for match {MatchId}", matchId);
                    break;

                case "disputed":
                    await _scoringChannel.QueueMatchDisputedAsync(matchId, !match.IsDisputed, "scorer");
                    _logger.LogInformation("Queued match disputed toggle for match {MatchId}, new value: {IsDisputed}", matchId, !match.IsDisputed);
                    break;
                    
                default:
                    _logger.LogWarning("Unknown quick action type: {ActionType} for match {MatchId}", actionType, matchId);
                    break;
            }
            
            // Refresh match state after queuing
            match = await _matchService.GetMatchAsync(matchId);
            
            if (match == null)
            {
                _logger.LogWarning("Match {MatchId} not found after quick action {ActionType}", matchId, actionType);
                await Clients.Caller.SendAsync("ReceiveError", "Match not found after action");
                return;
            }

            await Clients.Group($"scorer_{matchId}").SendAsync("ReceiveMatchStateChange", new
            {
                MatchId = matchId,
                ActionType = actionType,
                IsFinished = match.IsFinished,
                IsDisputed = match.IsDisputed,
                IsLocked = match.IsLocked,
                ActualStartTime = match.ActualStartTime,
                CurrentSetNumber = match.CurrentSetNumber,
                Timestamp = DateTime.Now
            });
            _logger.LogInformation("Broadcasted match state change for match {MatchId} after action {ActionType}", matchId, actionType);

            // Also send feed update if an update was created
            if (update != null)
            {
                var updateDto = new
                {
                    update.Content,
                    update.CreatedAt,
                    update.UpdateType
                };
                await Clients.Group($"scorer_{matchId}").SendAsync("ReceiveFeedUpdate", updateDto);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing quick action {ActionType} for match {MatchId}", actionType, matchId);
            await Clients.Caller.SendAsync("ReceiveError", ex.Message);
        }
    }
    public async Task SendFeedList(Guid matchId)
    {
        try
        {
            _logger.LogInformation("Client {ConnectionId} requesting feed list for match {MatchId}", Context.ConnectionId, matchId);
            var updates = await _matchService.GetMatchUpdatesAsync(matchId);
            var updatesDto = updates.Take(20).Select(u => new
            {
                u.Content,
                u.CreatedAt,
                Timestamp = u.CreatedAt,
                u.UpdateType
            }).ToList();
            await Clients.Group($"scorer_{matchId}").SendAsync("ReceiveFeedList", updatesDto);
            _logger.LogInformation("Sent {Count} updates to scorer_{MatchId} group", updatesDto.Count, matchId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feed list for match {MatchId}", matchId);
            await Clients.Caller.SendAsync("ReceiveError", ex.Message);
        }
    }

    public async Task SendSetStateChange(Guid matchId, string actionType, int currentSetNumber)
    {
        try
        {
            _logger.LogInformation("Client {ConnectionId} sending set state change {ActionType} for match {MatchId}, set {SetNumber}",
                Context.ConnectionId, actionType, matchId, currentSetNumber);
            
            var match = await _matchService.GetMatchAsync(matchId);
            if (match == null)
            {
                _logger.LogWarning("Set state change failed: Match {MatchId} not found", matchId);
                await Clients.Caller.SendAsync("ReceiveError", "Match not found");
                return;
            }

            switch (actionType.ToLower())
            {
                case "endcurrentset":
                    if (currentSetNumber == match.CurrentSetNumber)
                    {
                        await _scoringChannel.QueueMatchSetEndAsync(matchId, currentSetNumber, "scorer");
                        _logger.LogInformation("Queued end current set for match {MatchId}, set {SetNumber}", matchId, currentSetNumber);
                    }
                    else
                    {
                        _logger.LogWarning("Cannot end set {SetNumber}, current set is {CurrentSetNumber} for match {MatchId}",
                            currentSetNumber, match.CurrentSetNumber, matchId);
                    }
                    break;

                case "startnextset":
                    if (currentSetNumber == match.CurrentSetNumber)
                    {
                        await _scoringChannel.QueueMatchSetStartAsync(matchId, currentSetNumber + 1, "scorer");
                        _logger.LogInformation("Queued start next set for match {MatchId}, new set {NextSetNumber}", matchId, currentSetNumber + 1);
                    }
                    else
                    {
                        _logger.LogWarning("Cannot start next set, set {SetNumber} is not current set {CurrentSetNumber} for match {MatchId}",
                            currentSetNumber, match.CurrentSetNumber, matchId);
                    }
                    break;

                case "reverttopreviousset":
                    await _scoringChannel.QueueMatchSetRevertToPreviousAsync(matchId, currentSetNumber, "scorer");
                    _logger.LogInformation("Queued revert to previous set for match {MatchId}, from set {SetNumber}", matchId, currentSetNumber);
                    break;
                    
                default:
                    _logger.LogWarning("Unknown set state change action type: {ActionType} for match {MatchId}", actionType, matchId);
                    break;
            }
            
            // Broadcast optimistic state change (actual update will come from worker)
            await BroadcastSetStateChangeAsync(matchId, actionType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing set state change {ActionType} for match {MatchId}, set {SetNumber}", actionType, matchId, currentSetNumber);
            await Clients.Caller.SendAsync("ReceiveError", ex.Message);
        }
    }

    private async Task BroadcastSetStateChangeAsync(Guid matchId, string actionType)
    {
        try
        {
            _logger.LogDebug("Broadcasting set state change for match {MatchId}, action {ActionType}", matchId, actionType);
            
            // Get updated match and all sets
            var match = await _matchService.GetMatchAsync(matchId);
            var allSets = await _matchService.GetMatchSetsAsync(matchId);

            // Calculate sets won
            int homeSetsWon = allSets.Count(s => s.IsFinished && s.HomeTeamScore > s.AwayTeamScore);
            int awaySetsWon = allSets.Count(s => s.IsFinished && s.AwayTeamScore > s.HomeTeamScore);

            await Clients.Group($"scorer_{matchId}").SendAsync("ReceiveSetStateChange", new
            {
                MatchId = matchId,
                ActionType = actionType,
                CurrentSetNumber = match?.CurrentSetNumber ?? 0,
                Sets = allSets.Select(s => new
                {
                    s.SetNumber,
                    s.HomeTeamScore,
                    s.AwayTeamScore,
                    s.IsFinished,
                    s.IsLocked
                }),
                HomeSetsWon = homeSetsWon,
                AwaySetsWon = awaySetsWon,
                Timestamp = DateTime.Now
            });
            
            _logger.LogInformation("Broadcasted set state change to scorer_{MatchId} group: CurrentSet={CurrentSetNumber}, HomeSetsWon={HomeSetsWon}, AwaySetsWon={AwaySetsWon}",
                matchId, match?.CurrentSetNumber ?? 0, homeSetsWon, awaySetsWon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting set state change for match {MatchId}", matchId);
        }
    }

    public async Task BroadcastScoreUpdate(Guid matchId)
    {
        try
        {
            _logger.LogDebug("Broadcasting score update for match {MatchId}", matchId);
            
            var match = await _matchService.GetMatchAsync(matchId);
            if (match == null)
            {
                _logger.LogWarning("Cannot broadcast score update: Match {MatchId} not found", matchId);
                return;
            }

            var currentSet = await _matchService.GetOrCreateMatchSetAsync(matchId, match.CurrentSetNumber, "scorer");

            if (currentSet == null)
            {
                _logger.LogWarning("Cannot broadcast score update: Current set not found for match {MatchId}", matchId);
                return;
            }

            // Broadcast optimistic update (actual update will come from worker)
            await Clients.Group($"scorer_{matchId}").SendAsync("ReceiveScoreUpdate", new
            {
                MatchId = matchId,
                SetNumber = currentSet.SetNumber,
                HomeScore = currentSet.HomeTeamScore,
                AwayScore = currentSet.AwayTeamScore,
                HomeSetsWon = match.HomeTeamScore,
                AwaySetsWon = match.AwayTeamScore,
                Timestamp = DateTime.Now
            });
            
            _logger.LogInformation("Broadcasted score update to scorer_{MatchId} group: Set {SetNumber}, Home={HomeScore}, Away={AwayScore}",
                matchId, currentSet.SetNumber, currentSet.HomeTeamScore, currentSet.AwayTeamScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting score update for match {MatchId}", matchId);
            await Clients.Caller.SendAsync("ReceiveError", ex.Message);
        }
    }
    
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected to ScorerHub", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogError(exception, "Client {ConnectionId} disconnected from ScorerHub with error", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Client {ConnectionId} disconnected from ScorerHub", Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
