using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Hubs;

public class MatchHub : Hub
{
    public async Task JoinMatch(string matchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"match_{matchId}");
    }

    public async Task LeaveMatch(string matchId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"match_{matchId}");
    }

    public async Task JoinDivision(string division)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"division_{division}");
    }

    public async Task LeaveDivision(string division)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"division_{division}");
    }

    public async Task JoinRound(string roundId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"round_{roundId}");
    }

    public async Task LeaveRound(string roundId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"round_{roundId}");
    }

    public async Task JoinTeam(string teamId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"team_{teamId}");
    }

    public async Task LeaveTeam(string teamId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"team_{teamId}");
    }

    public async Task SubscribeToAnnouncements()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "announcements");
    }

    public async Task UnsubscribeFromAnnouncements()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "announcements");
    }

    public async Task SubscribeToUpdates()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "updates");
    }

    public async Task UnsubscribeFromUpdates()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "updates");
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "all");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "all");
        await base.OnDisconnectedAsync(exception);
    }

    // Server-side methods that can be called by clients
    public async Task SendMatchUpdate(MatchUpdate update)
    {
        await Clients.Group($"match_{update.MatchId}").SendAsync("ReceiveMatchUpdate", update);
        await Clients.Group("updates").SendAsync("ReceiveMatchUpdate", update);
    }

    public async Task SendScoreUpdate(Guid matchId, int homeScore, int awayScore)
    {
        var update = new
        {
            MatchId = matchId,
            HomeScore = homeScore,
            AwayScore = awayScore,
            Timestamp = DateTime.UtcNow
        };
        await Clients.Group($"match_{matchId}").SendAsync("ReceiveScoreUpdate", update);
        await Clients.Group("updates").SendAsync("ReceiveScoreUpdate", update);
    }

    public async Task SendAnnouncement(Announcement announcement)
    {
        await Clients.Group("announcements").SendAsync("ReceiveAnnouncement", announcement);
        await Clients.Group("all").SendAsync("ReceiveAnnouncement", announcement);
    }

    public async Task SendTeamUpdate(Team team)
    {
        await Clients.Group($"team_{team.Id}").SendAsync("ReceiveTeamUpdate", team);
        await Clients.Group($"division_{team.Division.Name}").SendAsync("ReceiveTeamUpdate", team);
    }

    public async Task SendRoundUpdate(Round round)
    {
        await Clients.Group($"round_{round.Id}").SendAsync("ReceiveRoundUpdate", round);
    }

    public async Task SendError(string error)
    {
        await Clients.Caller.SendAsync("ReceiveError", error);
    }

    public async Task BroadcastMessage(string message, string type = "info")
    {
        await Clients.All.SendAsync("ReceiveBroadcast", new { message, type });
    }
}
