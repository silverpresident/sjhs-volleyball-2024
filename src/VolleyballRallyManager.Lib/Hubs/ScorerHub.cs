using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.Lib.Hubs;

public class ScorerHub : Hub
{
    private readonly IMatchService _matchService;

    public ScorerHub(IMatchService matchService)
    {
        _matchService = matchService;
    }

    public async Task JoinMatchGroup(string matchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"scorer_{matchId}");
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

            var currentSet = await _matchService.GetCurrentSetAsync(matchId);
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

            var updatedSet = await _matchService.UpdateSetScoreAsync(matchId, setNumber, newHomeScore, newAwayScore, "scorer");

            await Clients.Group($"scorer_{matchId}").SendAsync("ReceiveScoreUpdate", new
            {
                MatchId = matchId,
                SetNumber = setNumber,
                HomeScore = updatedSet.HomeTeamScore,
                AwayScore = updatedSet.AwayTeamScore,
                Timestamp = DateTime.UtcNow
            });
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
                case "calltocourt":
                    update = new MatchUpdate
                    {
                        MatchId = matchId,
                        UpdateType = UpdateType.Other,
                        Content = $"{match.HomeTeam?.Name} vs {match.AwayTeam?.Name} called to court"
                    };
                    await _matchService.AddMatchUpdateAsync(update);
                    break;

                case "matchstarted":
                    await _matchService.StartMatchAsync(matchId, "scorer");
                    break;

                case "matchended":
                    await _matchService.EndMatchAndLockSetsAsync(matchId, "scorer");
                    break;

                case "disputed":
                    match.IsDisputed = !match.IsDisputed;
                    await _matchService.UpdateMatchAsync(match);
                    update = new MatchUpdate
                    {
                        MatchId = matchId,
                        UpdateType = UpdateType.DisputeRaised,
                        Content = match.IsDisputed ? "Match marked as disputed" : "Dispute cleared"
                    };
                    await _matchService.AddMatchUpdateAsync(update);
                    break;
            }

            await Clients.Group($"scorer_{matchId}").SendAsync("ReceiveMatchStateChange", new
            {
                MatchId = matchId,
                ActionType = actionType,
                IsFinished = match.IsFinished,
                IsDisputed = match.IsDisputed,
                ActualStartTime = match.ActualStartTime,
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

    public async Task SendSetStateChange(Guid matchId, string actionType)
    {
        try
        {
            var match = await _matchService.GetMatchAsync(matchId);
            if (match == null)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Match not found");
                return;
            }

            MatchSet? resultSet = null;

            switch (actionType.ToLower())
            {
                case "endcurrentset":
                    resultSet = await _matchService.FinishSetAsync(matchId, match.CurrentSetNumber, "scorer");
                    break;

                case "startnextset":
                    resultSet = await _matchService.StartNextSetAsync(matchId, "scorer");
                    break;

                case "reverttopreviousset":
                    resultSet = await _matchService.RevertToPreviousSetAsync(matchId, "scorer");
                    break;
            }

            // Get updated match and all sets
            match = await _matchService.GetMatchAsync(matchId);
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
