using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.Lib.Hubs;

public class TournamentHub : Hub
{
    private readonly IAnnouncementService _announcementService;

    public TournamentHub(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

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

    public async Task SubscribeToBulletins()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "bulletins");
    }

    public async Task UnsubscribeFromBulletins()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "bulletins");
    }

    public async Task SubscribeToUpdates()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "updates");
    }

    public async Task UnsubscribeFromUpdates()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "updates");
    }

    public async Task SubscribeToAnnouncer()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "announcer");
    }

    public async Task UnsubscribeFromAnnouncer()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "announcer");
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
            Timestamp = DateTime.Now
        };
        await Clients.Group($"match_{matchId}").SendAsync("ReceiveScoreUpdate", update);
        await Clients.Group("updates").SendAsync("ReceiveScoreUpdate", update);
    }

    public async Task SendBulletin(Bulletin bulletin)
    {
        await Clients.Group("bulletins").SendAsync("ReceiveBulletin", bulletin);
        await Clients.Group("all").SendAsync("ReceiveBulletin", bulletin);
    }

    public async Task SendTeamUpdate(Team team)
    {
        await Clients.Group($"team_{team.Id}").SendAsync("ReceiveTeamUpdate", team);
        //await Clients.Group($"division_{team.Division.Name}").SendAsync("ReceiveTeamUpdate", team);
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

    public async Task RequestForAnnouncements()
    {
        var announcements = await _announcementService.GetQueuedAnnouncementsAsync();
        await Clients.Caller.SendAsync("AnnouncementQueueChanged", announcements.ToList());
    }
}
