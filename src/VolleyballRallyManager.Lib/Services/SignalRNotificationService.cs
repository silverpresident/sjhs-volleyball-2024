using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Hubs;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<MatchHub> _matchHubContext;
    private readonly IHubContext<ScorerHub> _scorerHubContext;
    public SignalRNotificationService(IHubContext<MatchHub> hubContext, IHubContext<ScorerHub> scorerHubContext)
    {
        _matchHubContext = hubContext;
        _scorerHubContext = scorerHubContext;
    }

    public async Task NotifyMatchCreatedAsync(Match match)
    {
        await _matchHubContext.Clients.All.SendAsync("MatchCreated", match);
    }

    public async Task NotifyMatchUpdatedAsync(Match match)
    {
        await _matchHubContext.Clients.All.SendAsync("MatchUpdated", match);
    }

    public async Task NotifyMatchStartedAsync(Match match)
    {
        await _matchHubContext.Clients.All.SendAsync("MatchStarted", match);
    }

    public async Task NotifyMatchFinishedAsync(Match match)
    {
        await _matchHubContext.Clients.All.SendAsync("MatchFinished", match);
    }

    public async Task NotifyMatchDisputedAsync(Match match)
    {
        await _matchHubContext.Clients.All.SendAsync("MatchDisputed", match);
    }

    public async Task NotifyScoreUpdateAsync(Match match)
    {
        await _matchHubContext.Clients.All.SendAsync("ScoreUpdated", match);
    }

    public async Task NotifyBulletinCreatedAsync(Bulletin bulletin)
    {
        await _matchHubContext.Clients.All.SendAsync("BulletinCreated", bulletin);
    }

    public async Task NotifyBulletinUpdatedAsync(Bulletin bulletin)
    {
        await _matchHubContext.Clients.All.SendAsync("BulletinUpdated", bulletin);
    }

    public async Task NotifyBulletinDeletedAsync(Guid bulletinId)
    {
        await _matchHubContext.Clients.All.SendAsync("BulletinDeleted", bulletinId);
    }

    public async Task NotifyTeamCreatedAsync(Team team)
    {
        await _matchHubContext.Clients.All.SendAsync("TeamCreated", team);
    }

    public async Task NotifyTeamUpdatedAsync(Team team)
    {
        await _matchHubContext.Clients.All.SendAsync("TeamUpdated", team);
    }

    public async Task NotifyTeamDeletedAsync(Guid teamId)
    {
        await _matchHubContext.Clients.All.SendAsync("TeamDeleted", teamId);
    }

    public async Task NotifyRoundStartedAsync(Round round)
    {
        await _matchHubContext.Clients.All.SendAsync("RoundStarted", round);
    }

    public async Task NotifyRoundFinishedAsync(Round round)
    {
        await _matchHubContext.Clients.All.SendAsync("RoundFinished", round);
    }

    public async Task NotifyTournamentStatusAsync(string status)
    {
        await _matchHubContext.Clients.All.SendAsync("TournamentStatus", status);
    }

    public async Task NotifyErrorAsync(string error)
    {
        await _matchHubContext.Clients.All.SendAsync("Error", error);
    }

    public async Task BroadcastMessageAsync(string message, string type = "info")
    {
        await _matchHubContext.Clients.All.SendAsync("BroadcastMessage", new { message, type });
    }

    public async Task NotifyAddFeedAsync(MatchUpdate update)
    { 
        if (update != null)
        {
            var matchId = update.MatchId;
            await _scorerHubContext.Clients.Group($"scorer_{matchId}").SendAsync("ReceiveFeedUpdate", update);
            await _matchHubContext.Clients.Group($"match_{matchId}").SendAsync("ReceiveFeedUpdate", update);
        }
    }
}
