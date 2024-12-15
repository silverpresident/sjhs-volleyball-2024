using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Hubs;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<MatchHub> _hubContext;

    public SignalRNotificationService(IHubContext<MatchHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyMatchCreatedAsync(Match match)
    {
        await _hubContext.Clients.All.SendAsync("MatchCreated", match);
    }

    public async Task NotifyMatchUpdatedAsync(Match match)
    {
        await _hubContext.Clients.All.SendAsync("MatchUpdated", match);
    }

    public async Task NotifyMatchStartedAsync(Match match)
    {
        await _hubContext.Clients.All.SendAsync("MatchStarted", match);
    }

    public async Task NotifyMatchFinishedAsync(Match match)
    {
        await _hubContext.Clients.All.SendAsync("MatchFinished", match);
    }

    public async Task NotifyMatchDisputedAsync(Match match)
    {
        await _hubContext.Clients.All.SendAsync("MatchDisputed", match);
    }

    public async Task NotifyScoreUpdateAsync(Match match)
    {
        await _hubContext.Clients.All.SendAsync("ScoreUpdated", match);
    }

    public async Task NotifyAnnouncementCreatedAsync(Announcement announcement)
    {
        await _hubContext.Clients.All.SendAsync("AnnouncementCreated", announcement);
    }

    public async Task NotifyAnnouncementUpdatedAsync(Announcement announcement)
    {
        await _hubContext.Clients.All.SendAsync("AnnouncementUpdated", announcement);
    }

    public async Task NotifyAnnouncementDeletedAsync(Guid announcementId)
    {
        await _hubContext.Clients.All.SendAsync("AnnouncementDeleted", announcementId);
    }

    public async Task NotifyTeamCreatedAsync(Team team)
    {
        await _hubContext.Clients.All.SendAsync("TeamCreated", team);
    }

    public async Task NotifyTeamUpdatedAsync(Team team)
    {
        await _hubContext.Clients.All.SendAsync("TeamUpdated", team);
    }

    public async Task NotifyTeamDeletedAsync(Guid teamId)
    {
        await _hubContext.Clients.All.SendAsync("TeamDeleted", teamId);
    }

    public async Task NotifyRoundStartedAsync(Round round)
    {
        await _hubContext.Clients.All.SendAsync("RoundStarted", round);
    }

    public async Task NotifyRoundFinishedAsync(Round round)
    {
        await _hubContext.Clients.All.SendAsync("RoundFinished", round);
    }

    public async Task NotifyTournamentStatusAsync(string status)
    {
        await _hubContext.Clients.All.SendAsync("TournamentStatus", status);
    }

    public async Task NotifyErrorAsync(string error)
    {
        await _hubContext.Clients.All.SendAsync("Error", error);
    }

    public async Task BroadcastMessageAsync(string message, string type = "info")
    {
        await _hubContext.Clients.All.SendAsync("BroadcastMessage", new { message, type });
    }
}
