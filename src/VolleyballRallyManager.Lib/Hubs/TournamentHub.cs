using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Models;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.Lib.Hubs;

public class TournamentHub : Hub
{
    private readonly IAnnouncementService _announcementService;
    private readonly ILogger<TournamentHub> _logger;

    public TournamentHub(IAnnouncementService announcementService, ILogger<TournamentHub> logger)
    {
        _announcementService = announcementService;
        _logger = logger;
    }

    public async Task JoinMatch(string matchId)
    {
        _logger.LogInformation("Client {ConnectionId} joining match group: match_{MatchId}", Context.ConnectionId, matchId);
        await Groups.AddToGroupAsync(Context.ConnectionId, $"match_{matchId}");
    }

    public async Task LeaveMatch(string matchId)
    {
        _logger.LogInformation("Client {ConnectionId} leaving match group: match_{MatchId}", Context.ConnectionId, matchId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"match_{matchId}");
    }

    public async Task JoinDivision(string division)
    {
        _logger.LogInformation("Client {ConnectionId} joining division group: division_{Division}", Context.ConnectionId, division);
        await Groups.AddToGroupAsync(Context.ConnectionId, $"division_{division}");
    }

    public async Task LeaveDivision(string division)
    {
        _logger.LogInformation("Client {ConnectionId} leaving division group: division_{Division}", Context.ConnectionId, division);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"division_{division}");
    }

    public async Task JoinRound(string roundId)
    {
        _logger.LogInformation("Client {ConnectionId} joining round group: round_{RoundId}", Context.ConnectionId, roundId);
        await Groups.AddToGroupAsync(Context.ConnectionId, $"round_{roundId}");
    }

    public async Task LeaveRound(string roundId)
    {
        _logger.LogInformation("Client {ConnectionId} leaving round group: round_{RoundId}", Context.ConnectionId, roundId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"round_{roundId}");
    }

    public async Task JoinTeam(string teamId)
    {
        _logger.LogInformation("Client {ConnectionId} joining team group: team_{TeamId}", Context.ConnectionId, teamId);
        await Groups.AddToGroupAsync(Context.ConnectionId, $"team_{teamId}");
    }

    public async Task LeaveTeam(string teamId)
    {
        _logger.LogInformation("Client {ConnectionId} leaving team group: team_{TeamId}", Context.ConnectionId, teamId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"team_{teamId}");
    }

    public async Task SubscribeToBulletins()
    {
        _logger.LogInformation("Client {ConnectionId} subscribing to bulletins", Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, "bulletins");
    }

    public async Task UnsubscribeFromBulletins()
    {
        _logger.LogInformation("Client {ConnectionId} unsubscribing from bulletins", Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "bulletins");
    }

    public async Task SubscribeToUpdates()
    {
        _logger.LogInformation("Client {ConnectionId} subscribing to updates", Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, "updates");
    }

    public async Task UnsubscribeFromUpdates()
    {
        _logger.LogInformation("Client {ConnectionId} unsubscribing from updates", Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "updates");
    }

    public async Task SubscribeToAnnouncer()
    {
        _logger.LogInformation("Client {ConnectionId} subscribing to announcer", Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, "announcer");
    }

    public async Task UnsubscribeFromAnnouncer()
    {
        _logger.LogInformation("Client {ConnectionId} unsubscribing from announcer", Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "announcer");
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected to TournamentHub", Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, "all");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogError(exception, "Client {ConnectionId} disconnected from TournamentHub with error", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Client {ConnectionId} disconnected from TournamentHub", Context.ConnectionId);
        }
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "all");
        await base.OnDisconnectedAsync(exception);
    }

    // Server-side methods that can be called by clients
    public async Task SendMatchUpdate(MatchUpdate update)
    {
        _logger.LogInformation("Sending match update for match {MatchId} to match_{MatchId} and updates groups", update.MatchId, update.MatchId);
        await Clients.Group($"match_{update.MatchId}").SendAsync("ReceiveMatchUpdate", update);
        await Clients.Group("updates").SendAsync("ReceiveMatchUpdate", update);
    }

    public async Task SendScoreUpdate(Guid matchId, int homeScore, int awayScore)
    {
        _logger.LogInformation("Sending score update for match {MatchId}: Home={HomeScore}, Away={AwayScore}", matchId, homeScore, awayScore);
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
        _logger.LogInformation("Broadcasting bulletin {BulletinId}: {Content}", bulletin.Id, bulletin.Content);
        await Clients.Group("bulletins").SendAsync("ReceiveBulletin", bulletin);
        await Clients.Group("all").SendAsync("ReceiveBulletin", bulletin);
    }

    public async Task SendTeamUpdate(Team team)
    {
        _logger.LogInformation("Sending team update for team {TeamId}: {TeamName}", team.Id, team.Name);
        await Clients.Group($"team_{team.Id}").SendAsync("ReceiveTeamUpdate", team);
        //await Clients.Group($"division_{team.Division.Name}").SendAsync("ReceiveTeamUpdate", team);
    }

    public async Task SendRoundUpdate(RoundTemplate round)
    {
        _logger.LogInformation("Sending round update for round {RoundId}: {RoundName}", round.Id, round.Name);
        await Clients.Group($"round_{round.Id}").SendAsync("ReceiveRoundUpdate", round);
    }

    public async Task SendError(string error)
    {
        _logger.LogWarning("Sending error to client {ConnectionId}: {Error}", Context.ConnectionId, error);
        await Clients.Caller.SendAsync("ReceiveError", error);
    }

    public async Task BroadcastMessage(string message, string type = "info")
    {
        _logger.LogInformation("Broadcasting message to all clients: Type={Type}, Message={Message}", type, message);
        await Clients.All.SendAsync("ReceiveBroadcast", new { message, type });
    }

    public async Task RequestForAnnouncements()
    {
        try
        {
            _logger.LogInformation("Client {ConnectionId} requesting announcements", Context.ConnectionId);
            var announcements = await _announcementService.GetQueuedAnnouncementsAsync();
            await Clients.Caller.SendAsync("AnnouncementQueueChanged", announcements.ToList());
            _logger.LogInformation("Sent {Count} announcements to client {ConnectionId}", announcements.Count(), Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving announcements for client {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("ReceiveError", "Failed to retrieve announcements");
        }
    }
}
