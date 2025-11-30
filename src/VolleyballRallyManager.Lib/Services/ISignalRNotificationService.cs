using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public interface ISignalRNotificationService
{
    Task NotifyMatchCreatedAsync(Match match);
    Task NotifyMatchUpdatedAsync(Match match);
    Task NotifyMatchStartedAsync(Match match);
    Task NotifyMatchFinishedAsync(Match match);
    Task NotifyMatchDisputedAsync(Match match);
    Task NotifyScoreUpdateAsync(Match match);
    Task NotifyAnnouncementCreatedAsync(Announcement announcement);
    Task NotifyAnnouncementUpdatedAsync(Announcement announcement);
    Task NotifyAnnouncementDeletedAsync(Guid announcementId);
    Task NotifyTeamCreatedAsync(Team team);
    Task NotifyTeamUpdatedAsync(Team team);
    Task NotifyTeamDeletedAsync(Guid teamId);
    Task NotifyRoundStartedAsync(Round round);
    Task NotifyRoundFinishedAsync(Round round);
    Task NotifyTournamentStatusAsync(string status);
    Task NotifyErrorAsync(string error);
    Task BroadcastMessageAsync(string message, string type = "info");
    Task NotifyAddFeedAsync(MatchUpdate update);
}
