using Microsoft.AspNetCore.SignalR;
using VolleyballRallyManager.Lib.Hubs;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Services;

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IMatchService _matchService;
    private readonly IHubContext<TournamentHub> _tournamentHubContext;
    private readonly IHubContext<ScorerHub> _scorerHubContext;
    public SignalRNotificationService(IHubContext<TournamentHub> hubContext, IHubContext<ScorerHub> scorerHubContext, IMatchService matchService)
    {
        _tournamentHubContext = hubContext;
        _scorerHubContext = scorerHubContext;
        _matchService = matchService;
    }

    private async Task<object> GetMatchDtoAsync(Match match)
    {
        var sets = await _matchService.GetMatchSetsAsync(match.Id);
        return  new
        {
            MatchId = match.Id,
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
        };
    }

    public async Task NotifyMatchCreatedAsync(Match match)
    {
        await _tournamentHubContext.Clients.All.SendAsync("MatchCreated", await GetMatchDtoAsync(match));
    }

    public async Task NotifyMatchUpdatedAsync(Match match)
    {
        await _tournamentHubContext.Clients.All.SendAsync("MatchUpdated", await GetMatchDtoAsync(match));
    }

    public async Task NotifyMatchStartedAsync(Match match)
    {
        await _tournamentHubContext.Clients.All.SendAsync("MatchStarted", await GetMatchDtoAsync(match));
    }

    public async Task NotifyMatchFinishedAsync(Match match)
    {
        await _tournamentHubContext.Clients.All.SendAsync("MatchFinished", await GetMatchDtoAsync(match));
    }

    public async Task NotifyMatchDisputedAsync(Match match)
    {
        await _tournamentHubContext.Clients.All.SendAsync("MatchDisputed", await GetMatchDtoAsync(match));
    }

    public async Task NotifyScoreUpdateAsync(Match match)
    {
        await _tournamentHubContext.Clients.All.SendAsync("ScoreUpdated", await GetMatchDtoAsync(match));
    }

    public async Task NotifyBulletinCreatedAsync(Bulletin bulletin)
    {
        var bulletinDto = new
        {
            bulletin.Id,
            bulletin.Content,
            bulletin.RenderedContent,
            bulletin.UseMarkdown,
            bulletin.Priority,
            bulletin.IsVisible,
            bulletin.CreatedAt,
            bulletin.UpdatedAt
        };
        await _tournamentHubContext.Clients.All.SendAsync("BulletinCreated", bulletinDto);
    }

    public async Task NotifyBulletinUpdatedAsync(Bulletin bulletin)
    {
        var bulletinDto = new
        {
            bulletin.Id,
            bulletin.Content,
            bulletin.RenderedContent,
            bulletin.UseMarkdown,
            bulletin.Priority,
            bulletin.IsVisible,
            bulletin.CreatedAt,
            bulletin.UpdatedAt
        };
        await _tournamentHubContext.Clients.All.SendAsync("BulletinUpdated", bulletinDto);
    }

    public async Task NotifyBulletinDeletedAsync(Guid bulletinId)
    {
        await _tournamentHubContext.Clients.All.SendAsync("BulletinDeleted", bulletinId);
    }

    public async Task NotifyTeamCreatedAsync(Team team)
    {
        var safeTeam = new
        {
            team.Id,
            team.Name,
            team.School,
            team.Color,
            team.LogoUrl
        };
        await _tournamentHubContext.Clients.All.SendAsync("TeamCreated", safeTeam);
    }

    public async Task NotifyTeamUpdatedAsync(Team team)
    {
        var safeTeam = new
        {
            team.Id,
            team.Name,
            team.School,
            team.Color,
            team.LogoUrl
        };
        await _tournamentHubContext.Clients.All.SendAsync("TeamUpdated", safeTeam);
    }

    public async Task NotifyTeamDeletedAsync(Guid teamId)
    {
        await _tournamentHubContext.Clients.All.SendAsync("TeamDeleted", teamId);
    }

    public async Task NotifyTournamentStatusAsync(string status)
    {
        await _tournamentHubContext.Clients.All.SendAsync("TournamentStatus", status);
    }

    public async Task NotifyErrorAsync(string error)
    {
        await _tournamentHubContext.Clients.All.SendAsync("Error", error);
    }

    public async Task BroadcastMessageAsync(string message, string type = "info")
    {
        await _tournamentHubContext.Clients.All.SendAsync("BroadcastMessage", new { message, type });
    }

    public async Task NotifyAddFeedAsync(MatchUpdate update)
    { 
        if (update != null)
        {
            var matchId = update.MatchId;
            var safeUpdate = new
            {
                update.Id,
                update.MatchId,
                update.Content,
                update.UpdateType,
                update.PreviousValue,
                update.NewValue,
                update.IsProcessed,
                update.ProcessedAt,
                update.CreatedAt
            };
            await _scorerHubContext.Clients.Group($"scorer_{matchId}").SendAsync("ReceiveFeedUpdate", safeUpdate);
            await _tournamentHubContext.Clients.Group($"match_{matchId}").SendAsync("ReceiveFeedUpdate", safeUpdate);
        }
    }

    private async Task<object> GetAnnouncementDtoAsync(Announcement announcement)
    {
        return new
        {
            AnnouncementId = announcement.Id,
            announcement.Id,
            announcement.Title,
            announcement.Content,
            announcement.Priority,
            announcement.SequencingNumber,
            announcement.FirstAnnouncementTime,
            announcement.LastAnnouncementTime,
            announcement.RemainingRepeatCount,
            announcement.AnnouncedCount,
            announcement.IsHidden,
            announcement.Tag,
            announcement.TournamentId,
            announcement.CreatedAt,
            announcement.UpdatedAt,
            Timestamp = DateTime.Now
        };
    }
    public async Task NotifyAnnouncementCreatedAsync(Announcement announcement)
    { 
        await _tournamentHubContext.Clients.Group("announcer").SendAsync("AnnouncementCreated", await GetAnnouncementDtoAsync(announcement));
    }

    public async Task NotifyAnnouncementUpdatedAsync(Announcement announcement)
    {
        await _tournamentHubContext.Clients.Group("announcer").SendAsync("AnnouncementUpdated", await GetAnnouncementDtoAsync(announcement));
    }

    public async Task NotifyAnnouncementDeletedAsync(Guid announcementId)
    {
        await _tournamentHubContext.Clients.Group("announcer").SendAsync("AnnouncementDeleted", announcementId);
    }

    public async Task NotifyAnnouncementQueueChangedAsync(List<Announcement> announcements)
    {
        var announcementsDto = announcements.Select(async a => await GetAnnouncementDtoAsync(a)).ToList();
        await _tournamentHubContext.Clients.Group("announcer").SendAsync("AnnouncementQueueChanged", announcementsDto);
    }

    public async Task NotifyAnnouncementCalledAsync(Announcement announcement)
    {
        await _tournamentHubContext.Clients.Group("announcer").SendAsync("AnnouncementCalled", await GetAnnouncementDtoAsync(announcement));
    }

    public async Task NotifyAnnouncementPropertyChangedAsync(Guid announcementId, string property, string value)
    {
        await _tournamentHubContext.Clients.Group("announcer").SendAsync("AnnouncementPropertyChanged", announcementId, property, value);
    }
}
