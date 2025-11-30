using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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

    public ScorerHub(IMatchService matchService, ScoringChannel scoringChannel)
    {
        _matchService = matchService;
        _scoringChannel = scoringChannel;
    }

    public async Task JoinMatchGroup(Guid matchId)
    {
        var match = await _matchService.GetMatchAsync(matchId);
        if (match == null)
        {
            await Clients.Caller.SendAsync("ReceiveError", "Match not found");
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, $"scorer_{matchId}");


        var sets = await _matchService.GetMatchSetsAsync(matchId);
        await Clients.Caller.SendAsync("ReceiveMatchState", new
        {
            MatchId = matchId,
            CurrentSetNumber = match?.CurrentSetNumber ?? 0,
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
            Timestamp = DateTime.UtcNow
        });


        var updates = await _matchService.GetMatchUpdatesAsync(matchId);
        var updatesDto = updates.Take(20).Select(u => new
        {
            Content = u.Content,
            CreatedAt = u.CreatedAt,
            Timestamp = u.CreatedAt,
            UpdateType = u.UpdateType
        }).ToList();
        await Clients.Caller.SendAsync("ReceiveFeedList", updatesDto);

    }

    public async Task LeaveMatchGroup(string matchId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"scorer_{matchId}");
    }

    public async Task SendScoreUpdate(Guid matchId, int setNumber, Guid teamId, int incrementValue)
    {
        try
        {
            var match = await _matchService.GetMatchAsync(matchId);
            if (match == null)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Match not found");
                return;
            }

            var currentSet = await _matchService.GetOrCreateMatchSetAsync(matchId, setNumber, "scorer");

            if (currentSet == null || currentSet.SetNumber != setNumber)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Set not found or not current");
                return;
            }

            if (currentSet.IsLocked)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Set is locked");
                return;
            }

            // Queue the score change event
            await _scoringChannel.QueueMatchSetScoreChangeAsync(matchId, setNumber, teamId, incrementValue, "scorer");

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
            await Clients.Caller.SendAsync("ReceiveError", ex.Message);
        }
    }

    public async Task SendQuickAction(Guid matchId, string actionType)
    {
        try
        {
            var match = await _matchService.GetMatchAsync(matchId);
            if (match == null)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Match not found");
                return;
            }

            MatchUpdate? update = null;

            switch (actionType.ToLower())
            {
                case "calltosupport":
                    await _scoringChannel.QueueCallToSupportAsync(matchId, "scorer");
                    break;

                case "calltocourt":
                    await _scoringChannel.QueueCallToCourtAsync(matchId, "scorer");
                    break;

                case "matchstarted":
                    await _scoringChannel.QueueMatchStartAsync(matchId, "scorer");
                    break;

                case "matchended":
                    await _scoringChannel.QueueMatchEndAsync(matchId, "scorer");
                    break;

                case "disputed":
                    await _scoringChannel.QueueMatchDisputedAsync(matchId, !match.IsDisputed, "scorer");
                    break;
            }
            
            // Refresh match state after queuing
            match = await _matchService.GetMatchAsync(matchId);

            await Clients.Group($"scorer_{matchId}").SendAsync("ReceiveMatchStateChange", new
            {
                MatchId = matchId,
                ActionType = actionType,
                IsFinished = match.IsFinished,
                IsDisputed = match.IsDisputed,
                IsLocked = match.IsLocked,
                ActualStartTime = match.ActualStartTime,
                CurrentSetNumber = match?.CurrentSetNumber ?? 0,
                Timestamp = DateTime.UtcNow
            });

            // Also send feed update if an update was created
            if (update != null)
            {
                await Clients.Group($"scorer_{matchId}").SendAsync("ReceiveFeedUpdate", update);
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", ex.Message);
        }
    }
    public async Task SendFeedList(Guid matchId)
    {
        try
        {
            var updates = await _matchService.GetMatchUpdatesAsync(matchId);
            updates = updates.Take(20).ToList();
            await Clients.Group($"scorer_{matchId}").SendAsync("ReceiveFeedList", updates);

        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", ex.Message);
}
    }

    public async Task SendSetStateChange(Guid matchId, string actionType, int currentSetNumber)
    {
        try
        {
            var match = await _matchService.GetMatchAsync(matchId);
            if (match == null)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Match not found");
                return;
            }

            switch (actionType.ToLower())
            {
                case "endcurrentset":
                    if (currentSetNumber == match.CurrentSetNumber)
                    {
                        await _scoringChannel.QueueMatchSetEndAsync(matchId, currentSetNumber, "scorer");
                    }
                    break;

                case "startnextset":
                    if (currentSetNumber == match.CurrentSetNumber)
                    {
                        await _scoringChannel.QueueMatchSetStartAsync(matchId, currentSetNumber + 1, "scorer");
                    }
                    break;

                case "reverttopreviousset":
                    await _scoringChannel.QueueMatchSetRevertToPreviousAsync(matchId, currentSetNumber, "scorer");
                    break;
            }
            
            // Broadcast optimistic state change (actual update will come from worker)
            await BroadcastSetStateChangeAsync(matchId, actionType);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", ex.Message);
        }
    }

    private async Task BroadcastSetStateChangeAsync(Guid matchId, string actionType)
    {
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
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task BroadcastScoreUpdate(Guid matchId)
    {
        try
        {
            var match = await _matchService.GetMatchAsync(matchId);
            if (match == null)
            {
                return;
            }

            var currentSet = await _matchService.GetOrCreateMatchSetAsync(matchId, match.CurrentSetNumber, "scorer");

            if (currentSet == null)
            {
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
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", ex.Message);
        }
    }
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
